
if (window.Ext) {
    //Wrapper for a shared store. Waits for the shared store to be initialised before returning lookups for shared stores.

    //Creates a new aynch-friendly share store
    Ext.define("ED47.views.data.SharedStore", {
        extend: "Ext.util.Observable",

        constructor: function(config) {
            this.id = config.id;
            this.model = config.model;
            this.deleteConfirmation = config.deleteConfirmation;

            try { //Attempt to create the model to make sure it exists. If not show a friendly message to the developer.
                Ext.create(this.model);
            } catch(e) {
                console.error("Model seems to be missing for " + this.model + ". Please add [Model] attribute to the BusinessEntity.");
                throw e;
            }

            var defaultConfig = {
                model: this.model,
                storeId: this.id,
                addUpdateFunction: config.addUpdateFunction,
                data: ED47.views.Models[this.id] ? ED47.views.Models[this.id] : { data: [] },
                proxy: {
                    type: "memory",
                    reader: {
                        type: "json",
                        root: "data"
                    }
                }
            };

            Ext.apply(defaultConfig, config);

            ED47.views.data.SharedStore.superclass.constructor.call(this, config);

            this.store = Ext.create("ED47.views.data.Store", defaultConfig);
        }
    });

    ED47.Stores = ED47.views.data.SharedStore;

    //When a new store is added to the StoreManager, call the corresponding listeners.

    ED47.Stores.listeners = [];
    ED47.Stores.isInitialized = false;

    ED47.Stores.onNewStore = function(index, store, id) {
        var listeners = ED47.Stores.listeners;
        var listener = Enumerable.from(listeners).where("el => el.id == '" + id + "'").firstOrDefault();

        if (listener) {
            listeners.splice(listeners.indexOf(listener), 1);
            for (var i = 0; i < listener.items.length; i++) {
                listener.items[i].cb.apply(listener.items[i].scope, [store]);
            }

        }
    };

    //Looks up a store and listens until the store is available.
    ED47.Stores.get = function(storeId, callback, scope) {
        ED47.Stores.initialize();
        var store = Ext.data.StoreManager.lookup(storeId);
        if (store) {
            callback.apply(scope, [store]);
            return;
        }

        var listener = Enumerable.from(ED47.Stores.listeners).where("el => el.id == '" + storeId + "'").firstOrDefault();
        if (!listener) {
            listener = { id: storeId, items: [] };
            ED47.Stores.listeners.push(listener);
        }
        listener.items.push({ id: storeId, cb: callback, scope: scope });
    };


    ED47.Stores.initialize = function() {
        if (ED47.Stores.isInitialized) return;
        ED47.Stores.isInitialized = true;
        Ext.data.StoreManager.on("add", ED47.Stores.onNewStore);
    };

    //Set's up a ShareStore and triggers ready event when done.
    ED47.Stores.setup = function(id, name, addUpdateFunction, initNewFunction, deleteFunction, deleteConfirmation, preselectedRecordId) {
        ED47.views.Render.addEvents(id);

        var config = {
            model: name,
            id: id,
            addUpdateFunction: addUpdateFunction,
            initNewFunction: initNewFunction,
            deleteFunction: deleteFunction,
            deleteConfirmation: deleteConfirmation,
            preselectedRecordId: preselectedRecordId
        };

        var ready = function() {
            var store = new ED47.views.data.SharedStore(config);
            ED47.views.Render.fireEvent(id);
        };

        if (ED47.views.Render.isRendered)
            ready();
        else
            ED47.views.Render.on("ready", ready);
    };

    Ext.define("ED47.views.data.Store", {
        extend: "Ext.data.Store",

        constructor: function (config) {
            this.forms = [];
            this.addEvents("select");
            this.addEvents("selectMulti");
            this.addUpdateFunction = config.addUpdateFunction;
            this.initNewFunction = config.initNewFunction;
            this.deleteFunction = config.deleteFunction;
            this.preselectedRecordId = config.preselectedRecordId;

            ED47.views.data.Store.superclass.constructor.call(this, config);

            if (this.addUpdateFunction != null)
                this.setupAutoSave();

            this.on("select", this.onSelect);
            this.on("selectMulti", this.onSelectMulti);
        },

        initNew: function (params) {
            var me = this;
            if (!this.initNewFunction) return;

            this.initNewFunction(params, function (callResult) {
                var r = callResult.data.ResultData.Item;
                me._updating = true;
                me.insert(0, r);
                me._updating = false;
                me.select(me, me.getAt(0));
                ED47.views.current.fireEvent("startedit");
            });
        },
        select: function (sender, record) {
            var view = this;
            Ext.each(view.forms, function (form) {
                form.owner.setDisabled(false);
                if (record)
                    form.loadRecord(record);
                else {
                    form.reset();
                }
            });
            this.fireEvent("select", sender, record, this);
        },
        onSelect: function (sender, record, context) {
            this.selectedRecord = record;
        },
        selectMulti: function (sender, records) {
            var view = this;
            Ext.each(view.forms, function (form) {
                if (records && records.length == 1) {
                    form.owner.setDisabled(false);
                    form.loadRecord(records[0]);
                } else {
                    form.reset();
                    form.owner.setDisabled(true);
                }
            });
            this.fireEvent("selectMulti", sender, records, this);
        },
        onSelectMulti: function (sender, records, context) {
            this.selectedRecords = records;
        },

        setupAutoSave: function () {
            //Wire sending updates to server when store it updated
            this.on("update", this.onUpdate, this);
            this.on("add", this.onUpdate, this);
        },

        //Called when there is a update.
        onUpdate: function (store, record, operation, modifiedFieldNames) {
            if (this._updating) return;


            if (record.length > 1) {
                return;
            }
            //Ignore mass adds

            if (record.length == 1)
                record = record[0];

            var view = this;

            if (!record.isValid()) {
                return;
            }

            Ext.each(modifiedFieldNames, function (fiedName) {
                Ext.each(view.forms, function (form) {
                    Ext.each(form.getFields().items, function (field) {
                        if (field.name == fiedName)
                            field.setDisabled(true);
                    });
                });
            });

            if (_.isEqual(record.data, this.serverValue)) {
                Ext.each(modifiedFieldNames, function (fiedName) {
                    Ext.each(view.forms, function (form) {
                        Ext.each(form.getFields().items, function (field) {
                            if (field.name == fiedName)
                                field.setDisabled(false);
                        });
                    });
                });

                return;
            }

            this.addUpdateFunction(record.data, function (callResult) {
                view.serverValue = callResult.data.ResultData.Item;
                view._updating = true;
                view.updateRecord(store, record, callResult);
                view._updating = false;

                Ext.each(modifiedFieldNames, function (fiedName) {
                    Ext.each(view.forms, function (form) {
                        Ext.each(form.getFields().items, function (field) {
                            if (field.name == fiedName)
                                field.setDisabled(false);
                        });
                    });
                });

            }, this);
        },

        updateRecords: function (data, idField) {
            for (var i = 0, max = data.length; i < max; i++) {
                var d = data[i];
                var r = this.getById(d[idField]);
                if (!r) continue;
                for (var p in d) {
                    r.set(p, d[p]);
                }
                r.commit();

            }
        },

        //Updates a record from a callResult.
        updateRecord: function (store, record, callResult) {
            var index = record.index || store.indexOf(record);

            store.removeAt(index);
            this.serverValue = Ext.applyIf(callResult.data.ResultData.Item, record.data);
            this._updating = true;
            store.insert(index, callResult.data.ResultData.Item);
            this._updating = false;
            this.select(this, store.getAt(index));
        },

        deleteRecord: function (record, callback) {
            var me = this;
            if (!this.deleteFunction) return;

            if (!this.deleteConfirmation) {
                me.deleteFunction(record.data, function (callResult) {
                    var r = callResult.data.ResultData.Item;
                    if (!r) {
                        Ext.Msg.alert('Delete Action not permited ', "You can't delete this item.");
                        return;
                    }
                    me.remove(record);
                    me.select(me, me.getAt(0));

                });
                if (callback) callback.call(this, true);
            } else {
                Ext.Msg.confirm("Management", "Remove selected item?", function (button) {
                    if (button === "yes") {
                        me.deleteFunction(record.data, function (callResult) {
                            var r = callResult.data.ResultData.Item;
                            if (!r) {
                                Ext.Msg.alert('Delete Action not permited ', "You can't delete this item.");
                                return;
                            }
                            me.remove(record);
                            me.select(me, me.getAt(0));

                        });
                    }
                    if (callback) callback.call(this, (button == "yes"));
                });
            }
        },

        deleteRecords: function (records, callback) {
            var me = this;
            if (!records) return;
            if (!this.deleteFunction) return;

            recordIds = Enumerable.from(records).select("el=>el.getId()").toArray();

            if (!this.deleteConfirmation) {
                me.deleteFunction(recordIds, function (callResult) {
                    var r = callResult.data.ResultData.Item;
                    if (!r) {
                        Ext.Msg.alert('Delete Action not permited ', "You can't delete this item.");
                        return;
                    }

                    Ext.each(records, function (record) {
                        me.remove(record);
                    });

                    me.select(me, me.getAt(0));

                });
                if (callback) callback.call(this, true);
            } else {
                Ext.Msg.confirm("Management", "Remove selected items?", function (button) {
                    if (button === "yes") {
                        me.deleteFunction(recordIds, function (callResult) {
                            var r = callResult.data.ResultData.Item;
                            if (!r) {
                                Ext.Msg.alert('Delete Action not permited ', "You can't delete this item.");
                                return;
                            }

                            Ext.each(records, function (record) {
                                me.remove(record);
                            });

                            me.select(me, me.getAt(0));

                        });
                    }
                    if (callback) callback.call(this, (button == "yes"));
                });
            }

        }
    });


    /**************************
    Tree Panel store
    ***************************/
    Ext.define("ED47.views.data.TreeStore", {
        extend: "Ext.data.TreeStore",

        constructor: function(config) {

            var defaultConfig = {
                model: config.model,
                storeId: config.id,
                proxy: {
                    type: "memory",
                    reader: {
                        type: "json"
                    }
                }
            };

            this.forms = [];
            this.addEvents("select");
            this.addUpdateFunction = config.addUpdateFunction;
            this.initNewFunction = config.initNewFunction;
            this.deleteFunction = config.deleteFunction;

            Ext.apply(defaultConfig, config);
            ED47.views.data.TreeStore.superclass.constructor.call(this, defaultConfig);

            if (this.addUpdateFunction != null)
                this.setupAutoSave();

            this.on("select", this.onSelect);
        },

        initNew: function(params) {
            var me = this;
            if (!this.initNewFunction) return;

            this.initNewFunction(params, function(callResult) {
                var r = callResult.data.ResultData.Item;
                me._updating = true;
                me.insert(0, r);
                me._updating = false;
                me.select(me, me.getAt(0));
                ED47.views.current.fireEvent("startedit");
            });
        },
        select: function(sender, record) {
            var view = this;
            Ext.each(view.forms, function(form) {
                form.owner.setDisabled(false);
                if (record)
                    form.loadRecord(record);
                else {
                    form.reset();
                }
            });
            this.fireEvent("select", sender, record, this);
        },
        onSelect: function(sender, record, context) {
            this.selectedRecord = record;
        },

        setupAutoSave: function() {
            //Wire sending updates to server when store it updated
            this.on("update", this.onUpdate, this);
            this.on("add", this.onUpdate, this);
        },

        //Called when there is a update.
        onUpdate: function(store, record, operation, modifiedFieldNames) {
            if (this._updating) return;


            if (record.length > 1) {
                return;
            }
            //Ignore mass adds

            if (record.length == 1)
                record = record[0];

            var view = this;

            if (!record.isValid()) {
                return;
            }

            Ext.each(modifiedFieldNames, function(fiedName) {
                Ext.each(view.forms, function(form) {
                    Ext.each(form.getFields().items, function(field) {
                        if (field.name == fiedName)
                            field.setDisabled(true);
                    });
                });
            });

            if (_.isEqual(record.data, this.serverValue)) {

                return;
            }

            this.addUpdateFunction(record.data, function(callResult) {
                view.serverValue = callResult.data.ResultData.Item;
                view._updating = true;
                view.updateRecord(store, record, callResult);
                view._updating = false;
                Ext.each(modifiedFieldNames, function(fiedName) {
                    Ext.each(view.forms, function(form) {
                        Ext.each(form.getFields().items, function(field) {
                            if (field.name == fiedName)
                                field.setDisabled(false);
                        });
                    });
                });

            }, this);
        },

        //Updates a record from a callResult.
        updateRecord: function(store, record, callResult) {
            var index = record.index || store.indexOf(record);

            store.removeAt(index);
            this.serverValue = Ext.applyIf(callResult.data.ResultData.Item, record.data);
            this._updating = true;
            store.insert(index, callResult.data.ResultData.Item);
            this._updating = false;
            this.select(this, store.getAt(index));
            console.log("Updated record.");
        },

        deleteRecord: function(record, callback) {
            var me = this;
            if (!this.deleteFunction) return;

            if (!this.deleteConfirmation) {
                me.deleteFunction(record.data, function(callResult) {
                    var r = callResult.data.ResultData.Item;
                    if (!r) {
                        Ext.Msg.alert('Delete Action not permited ', "You can't delete this item.");
                        return;
                    }
                    me.remove(record);
                    me.select(me, me.getAt(0));

                });
                if (callback) callback.call(this, true);
            } else {
                Ext.Msg.confirm("Management", "Remove selected item?", function(button) {
                    if (button === "yes") {
                        me.deleteFunction(record.data, function(callResult) {
                            var r = callResult.data.ResultData.Item;
                            if (!r) {
                                Ext.Msg.alert('Delete Action not permited ', "You can't delete this item.");
                                return;
                            }
                            me.remove(record);
                            me.select(me, me.getAt(0));

                        });
                    }
                    if (callback) callback.call(this, (button == "yes"));
                });
            }

        }
    });
}