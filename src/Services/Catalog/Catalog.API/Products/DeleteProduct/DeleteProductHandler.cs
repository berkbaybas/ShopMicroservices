using BuildingBlocks.CQRS;
using Catalog.API.Models;

namespace Catalog.API.Products.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductResult>;
    public record DeleteProductResult(bool IsSuccess);

    public class DeleteProductCommandValidatior : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidatior()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Product ID is required");
        }
    }

    internal class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, DeleteProductResult>
    {
        // Usage normal DI without Primary Constructor.
        IDocumentSession session;
        public DeleteProductCommandHandler(IDocumentSession session)
        {
            this.session = session;
        }
        public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            session.Delete<Product>(command.Id);
            await session.SaveChangesAsync(cancellationToken);

            return new DeleteProductResult(true);
        }
    }
}
