using System.Web.Mvc;

namespace ManejoPresupuesto.Models
{
    public class CuentasCreacionViewModel : Cuenta
    {
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}
