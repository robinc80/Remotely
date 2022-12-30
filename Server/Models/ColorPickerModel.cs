using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Remotely.Server.Models
{
    public class ColorPickerModel
    {
        [DisplayName("Rouge")]
        public byte Red { get; set; }

        [DisplayName("Vert")]
        public byte Green { get; set; }

        [DisplayName("Bleu")]
        public byte Blue { get; set; }
    }
}
