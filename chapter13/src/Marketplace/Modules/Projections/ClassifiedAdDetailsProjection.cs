using System;
using System.Threading.Tasks;
using Marketplace.Ads.Domain.ClassifiedAds;
using Marketplace.Infrastructure.RavenDb;
using Raven.Client.Documents.Session;

namespace Marketplace.Modules.Projections
{
    public class ClassifiedAdDetailsProjection : RavenDbProjection<ReadModels.ClassifiedAdDetails>
    {
        private readonly Func<Guid, Task<string>> _getUserDisplayName;

        public ClassifiedAdDetailsProjection(Func<IAsyncDocumentSession> getSession,
            Func<Guid, Task<string>> getUserDisplayName)
            : base(getSession)
        {
            _getUserDisplayName = getUserDisplayName;
        }

        public override async Task Project(object @event)
        {
            switch (@event)
            {
                case Events.ClassifiedAdCreated e:
                    await UsingSession(async session =>
                        await session.StoreAsync(new ReadModels.ClassifiedAdDetails
                        {
                            Id = e.Id.ToString(),
                            SellerId = e.OwnerId,
                            SellersDisplayName = await _getUserDisplayName(e.OwnerId)
                        }));
                    break;
                case Events.ClassifiedAdTitleChanged e:
                    await UsingSession(session =>
                        UpdateItem(session, e.Id, ad => ad.Title = e.Title));
                    break;
                case Events.ClassifiedAdTextUpdated e:
                    await UsingSession(session =>
                        UpdateItem(session, e.Id, ad => ad.Description = e.AdText));
                    break;
                case Events.ClassifiedAdPriceUpdated e:
                    await UsingSession(session =>
                        UpdateItem(session, e.Id, ad =>
                        {
                            ad.Price = e.Price;
                            ad.CurrencyCode = e.CurrencyCode;
                        }));
                    break;
                case Events.ClassifiedAdDeleted e:
                    await UsingSession(async session =>
                    {
                        var doc = await session.LoadAsync<ReadModels.ClassifiedAdDetails>(
                            e.Id.ToString());
                        session.Delete(doc);
                    });
                    break;
                case Ads.Domain.UserProfile.Events.UserDisplayNameUpdated e:
                    await UsingSession(session =>
                        UpdateMultipleItems(session, x => x.SellerId == e.UserId,
                            x => x.SellersDisplayName = e.DisplayName));
                    break;
                case ClassifiedAdUpcastedEvents.V1.ClassifiedAdPublished e:
                    await UsingSession(session =>
                        UpdateItem(session, e.Id, ad => ad.SellersPhotoUrl = e.SellersPhotoUrl));
                    break;
            }
        }
    }
}