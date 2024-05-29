namespace Homeworks.FamiyTree
{
    enum Gender { male, female }
    enum Status
    {
        married, //мужчина и женщина состоят в браке
        notMarried, //человек не состоит в официальных отношениях
        divorced, //человек был в браке, но сейчас не связан узами брака
        widowed //супруг или супруга умерли
    }
    internal class FamilyMember
    {
        private Status status;
        public string? FullName { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public FamilyMember Mother { get; set; }
        public FamilyMember Father { get; set; }
        public FamilyMember Husband { get; set; }
        public FamilyMember Wife { get; set; }
        public FamilyMember Son { get; set; }
        public FamilyMember Daughter { get; set; }
        public FamilyMember[] GetGrandMothers() => [Mother?.Mother, Father?.Mother];
        public FamilyMember[] GetGrandFathers() => [Mother?.Father, Father?.Father];

        private Status CheckStatus()
        {
            if (Husband == null && Wife == null)
            {
                if (Son == null && Daughter == null) return Status.notMarried;
                else return Status.widowed;
            }
            else return Status.married;

        }
        public FamilyMember GetPartner()
        {
            if (CheckStatus() == Status.married)
                return Gender == Gender.female ? Husband : Wife;
            else return null;
        }

        public void ShowPartner()
        {
            if(Gender == Gender.female) Console.WriteLine("Супруг -> " + Husband.FullName);
            else Console.WriteLine("Супруга -> " + Wife.FullName);
        }
    }

}
