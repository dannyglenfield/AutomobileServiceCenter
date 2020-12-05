using System;
using System.Threading.Tasks;
using AzureStorageTest;

//await CreateNewEntity();

//await FindAndUpdateEntity();

//await DeleteEntityRollback();

await DeleteEntity();


async Task CreateNewEntity()
{
    using (var unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
    {
        var bookRepository = unitOfWork.Repository<Book>();
        await bookRepository.CreateTableAsync();

        Book book = new Book(1, "APress")
        {
            Author = "Rami",
            BookName = "ASP.NET Core With Azure"
        };

        var data = await bookRepository.AddAsync(book);
        Console.WriteLine(data);

        unitOfWork.CommitTransaction();
    }
}

async Task DeleteEntity()
{
    using (var unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
    {
        var bookRepository = unitOfWork.Repository<Book>();
        await bookRepository.CreateTableAsync();

        var data = await bookRepository.FindAsync("APress", "1");
        Console.WriteLine(data);

        await bookRepository.DeleteAsync(data);
        Console.WriteLine("Deleted");

        unitOfWork.CommitTransaction();
    }
}

async Task DeleteEntityRollback()
{
    using (var unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
    {
        var bookRepository = unitOfWork.Repository<Book>();
        await bookRepository.CreateTableAsync();

        var data = await bookRepository.FindAsync("APress", "1");
        Console.WriteLine(data);

        await bookRepository.DeleteAsync(data);
        Console.WriteLine("Deleted");

        // Throw an exception to test rollback actions
        throw new Exception();

        unitOfWork.CommitTransaction();
    }
}

async Task FindAndUpdateEntity()
{
    using (var unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
    {
        var bookRepository = unitOfWork.Repository<Book>();
        await bookRepository.CreateTableAsync();

        var data = await bookRepository.FindAsync("APress", "1");
        Console.WriteLine(data);

        data.Author = "Rami Vemula";
        var updatedData = await bookRepository.UpdateAsync(data);
        Console.WriteLine(updatedData);

        unitOfWork.CommitTransaction();
    }
}