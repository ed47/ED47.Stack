using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using File = ED47.BusinessAccessLayer.BusinessEntities.File;

namespace ED47.BusinessAccessLayer
{
    public interface IFileRepository
    {
        bool Write(File file,  byte[] content);
        bool Append(File file, byte[] content);
        bool Delete(File file);
        bool Exist(File file);
        Stream OpenWrite(File file);
        Stream OpenRead(File file);
    }
}
