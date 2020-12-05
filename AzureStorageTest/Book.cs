using System.Text.Json;
using AzureStorageTest.Interfaces;

namespace AzureStorageTest
{
    public class Book : BaseEntity, IAuditTracker
    {
        public string Author { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Publisher { get; set; }

        public Book()
        {
        }

        public Book(int bookId, string publisher)
        {
            BookId = bookId;
            Publisher = publisher;
            this.PartitionKey = Publisher;
            this.RowKey = BookId.ToString();
        }

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
