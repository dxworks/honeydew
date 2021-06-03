using System;

namespace HoneydewCore.IO.Readers
{
     [Serializable]
     public class ProjectNotFoundException : Exception
     {
          public ProjectNotFoundException()
          {
          }

          public ProjectNotFoundException(string message) : base(message)
          {
          }
     }
}
