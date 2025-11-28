using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCI.AIAssistant.API.models.DTOs
{
    public class ErrorResponseDTO
    {
        public required string TextErrorTitle { get; set; }
        public required string TextErrorMessage { get; set; }
        public required string TextErrorTrace { get; set; }
    }
}