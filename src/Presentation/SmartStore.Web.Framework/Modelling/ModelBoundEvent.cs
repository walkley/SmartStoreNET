using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Http;


namespace SmartStore.Web.Framework.Modelling
{
    public class ModelBoundEvent
    {
        public ModelBoundEvent(TabbableModel boundModel, object entityModel, FormCollection form)
        {
            this.BoundModel = boundModel;
            this.EntityModel = entityModel;
            this.Form = form;
        }

        public TabbableModel BoundModel { get; private set; }
        public object EntityModel { get; private set; }
        public FormCollection Form { get; private set; }
    }
}
