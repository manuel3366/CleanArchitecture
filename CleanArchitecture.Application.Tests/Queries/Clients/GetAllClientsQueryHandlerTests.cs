using System;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Application.Queries.Clients.GetAll;
using CleanArchitecture.Application.SortProviders;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces.Repositories;
using MockQueryable;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CleanArchitecture.Application.Tests.Queries.Clients;

public sealed class GetAllClientsQueryHandlerTests
{
    [Fact]
    public async Task Should_Filter_Search_ExcludeDeleted_Order_And_Paginate_Projected_Clients()
    {
        var repository = Substitute.For<IClientRepository>();
        var alpha = new Client(Guid.NewGuid(), "Alpha", "alpha@example.test", "555-1111");
        var beta = new Client(Guid.NewGuid(), "Beta", "beta@example.test", "555-2222");
        var deleted = new Client(Guid.NewGuid(), "Deleted", "deleted@example.test", "555-3333");
        deleted.Delete();
        repository.GetAllNoTracking().Returns(new[] { alpha, beta, deleted }.BuildMock());

        var handler = new GetAllClientsQueryHandler(repository, new ClientViewModelSortProvider());
        var sortQuery = new SortQuery { Query = "name desc" };
        var result = await handler.Handle(
            new GetAllClientsQuery(
                new PageQuery { Page = 2, PageSize = 1 },
                IncludeDeleted: false,
                SearchTerm: "example.test",
                SortQuery: sortQuery),
            default);

        result.Count.ShouldBe(2);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(1);
        result.Items.ShouldHaveSingleItem();
        result.Items.Single().Id.ShouldBe(alpha.Id);
        result.Items.Single().Name.ShouldBe("Alpha");
        result.Items.Single().Email.ShouldBe("alpha@example.test");
        result.Items.Single().Phone.ShouldBe("555-1111");
        result.Items.Single().DeletedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Include_Deleted_And_Keep_Count_When_Page_Is_Out_Of_Range()
    {
        var repository = Substitute.For<IClientRepository>();
        var active = new Client(Guid.NewGuid(), "Active", "active@example.test", "555-1111");
        var deleted = new Client(Guid.NewGuid(), "Deleted", "deleted@example.test", "555-2222");
        deleted.Delete();
        repository.GetAllNoTracking().Returns(new[] { active, deleted }.BuildMock());

        var handler = new GetAllClientsQueryHandler(repository, new ClientViewModelSortProvider());
        var result = await handler.Handle(
            new GetAllClientsQuery(new PageQuery { Page = 3, PageSize = 1 }, IncludeDeleted: true),
            default);

        result.Count.ShouldBe(2);
        result.Items.ShouldBeEmpty();
        result.Page.ShouldBe(3);
        result.PageSize.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Return_Empty_Page_For_Very_Large_Page_Without_Overflowing()
    {
        var repository = Substitute.For<IClientRepository>();
        var client = new Client(Guid.NewGuid(), "Client", "client@example.test", "555-1111");
        repository.GetAllNoTracking().Returns(new[] { client }.BuildMock());

        var handler = new GetAllClientsQueryHandler(repository, new ClientViewModelSortProvider());
        var result = await handler.Handle(
            new GetAllClientsQuery(
                new PageQuery { Page = int.MaxValue, PageSize = 100 },
                IncludeDeleted: false),
            default);

        result.Count.ShouldBe(1);
        result.Items.ShouldBeEmpty();
        result.Page.ShouldBe(int.MaxValue);
        result.PageSize.ShouldBe(100);
    }
}
