using CMS.Core.DTOs;
using CMS.Core.Entities;

namespace MrLubeCMS.ViewModels
{
    public class VMLeftAd
    {
        public LeftAdDto? LeftAdDto { get; set; }
        public List<SubMenu>? SubMenus { get; set; }
    }
}
