namespace ED47.BusinessAccessLayer.Azure
{
    public interface IBlobItem
    {
        string BlobName { get; set; }
        string ContainerName { get; set; }
        string Url { get; set; }
       
    }
}
