using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaState.Estados
{
    public abstract class EstadoProceso
    {
        protected ProcesoContext _context;
        public void SetEstado(ProcesoContext context)
        {
            this._context = context;
        }

        public abstract void Iniciar();

        public abstract void Interrumpir();

        public abstract void Previsualizar();

        public abstract void Validar();

        public abstract void Exportar();

        public abstract void Finalizar();

        public abstract void Next();
    }
}
