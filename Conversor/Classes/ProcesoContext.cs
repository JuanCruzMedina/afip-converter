using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaState.Estados
{
    public class ProcesoContext
    {
        private EstadoProceso estado;
        public string mensaje { get; set; }
        public bool value { get; set; }
        public bool fin { get; set; }

        public string FilePath { get; set; }
        public string DefaultPath { get; set; }
        public bool ExtractionDefault { get; set; }
        public bool Export_To_Excel { get; set; }
        public bool Export_To_Txt { get; set; }

        public ProcesoContext(EstadoProceso estado, string filePath, string defaultPath, bool extractionDefault, bool export_To_Excel, bool export_To_Txt)
        {
            this.Transitar(estado);
            FilePath = filePath;
            DefaultPath = defaultPath;
            ExtractionDefault = extractionDefault;
            Export_To_Excel = export_To_Excel;
            Export_To_Txt = export_To_Txt;
        }

        public void Transitar(EstadoProceso state)
        {
            this.estado = state;
            this.estado.SetEstado(this);
        }
        public void Iniciar()
        {
            this.estado.Iniciar();
        }
        public void Finalizar()
        {
            this.estado.Finalizar();
        }
        public void Interrumpir()
        {
            this.estado.Interrumpir();
        }
        public void Previsualizar()
        {
            this.estado.Previsualizar();
        }
        public void Validar()
        {
            this.estado.Validar();
        }
        public void Exportar()
        {
            this.estado.Exportar();
        }
        public void Next()
        {
            this.estado.Next();
        }
    }
}
