﻿namespace HoneydewModels.Reference
{
    public class AccessedField : ReferenceEntity
    {
        public FieldModel Field { get; set; }
        
        public PropertyModel Property { get; set; }

        public string AccessType { get; set; }
    }
}
