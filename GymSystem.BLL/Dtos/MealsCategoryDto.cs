using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class MealsCategoryDto
    {
        public int MealsCategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
