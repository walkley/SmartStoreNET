using NUnit.Framework;
using SmartStore.Core.Localization;
using NUnit.Framework.Legacy;


namespace SmartStore.Web.MVC.Tests.Public.Validators
{
    [TestFixture]
    public abstract class BaseValidatorTests
    {
        protected Localizer T = NullLocalizer.Instance;

        [SetUp]
        public void Setup()
        {
        }
    }
}
