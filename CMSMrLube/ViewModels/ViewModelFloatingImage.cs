using CMS.Core.DTOs;
using CMS.Core.Entities;

namespace MrLubeCMS.ViewModels
{
    public class ViewModelFloatingImage
    {
        public FloatingImageDto? FloatingImageDto { get; set; }
        public List<SubMenu>? SubMenus { get; set; }
    }
}
