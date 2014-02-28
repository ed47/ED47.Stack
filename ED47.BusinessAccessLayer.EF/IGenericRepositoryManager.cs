namespace ED47.BusinessAccessLayer.EF
{
    public interface IGenericRepositoryManager : BusinessAccessLayer.IGenericRepositoryManager
    {
        new ISqlRepository Repository { get; }
    }
}