using System.ComponentModel.DataAnnotations;

namespace JSD.Api.Dtos;

// Data Transfer Object for Task used to grab the body request and validate it
//   NOTE: Honestly, im still a bit confused how .NET handles body requests autoamtically
//   in the controller, seems a bit like magic and feels like it should be more explicit
//   but I guess this is the way it is done in .NET maybe?
public class TaskDto
{
  [Required, MaxLength(255)]
  public string Description { get; set; } = "";
}