using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.FamiyTree
{
    enum Gender { male, female}
    internal class FamilyMember
    {
        public string FullName { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public FamilyMember Mother { get; set; }
        public FamilyMember Father { get; set; }
        public FamilyMember[] GetGrandMothers() => [Mother.Mother, Father.Mother];
        public FamilyMember[] GetGrandFathers() => [Mother.Father, Father.Father];
        
        
    }
}
