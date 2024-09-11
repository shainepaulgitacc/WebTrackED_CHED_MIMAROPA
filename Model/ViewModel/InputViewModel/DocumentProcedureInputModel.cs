using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class DocumentProcedureInputModel:BaseInputModel
    {
        public int DocumentAttachmentId { get; set; }
        public bool IsDone { get; set; }
        public string ProcedureDescription { get; set; }
    }
}
