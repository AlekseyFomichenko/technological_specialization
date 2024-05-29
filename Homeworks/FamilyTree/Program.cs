using Homeworks.FamiyTree;

namespace Homeworks.FamilyTree
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FamilyMember grandMother1 = new FamilyMember()
            {
                FullName = "Виноградова Варвара Максимовна",
                Age = 65,
                Gender = Gender.female,
            };

            FamilyMember grandFather1 = new FamilyMember()
            {
                FullName = "Виноградов Марк Аркадьевич",
                Age = 69,
                Gender = Gender.male,
            };

            FamilyMember grandMother2 = new FamilyMember()
            {
                FullName = "Чернова Виолета Васильевна",
                Age = 72,
                Gender = Gender.female,
            };

            FamilyMember grandFather2 = new FamilyMember()
            {
                FullName = "Чернов Тимофей Алексеевич",
                Age = 75,
                Gender = Gender.male,
            };

            FamilyMember mother = new FamilyMember()
            {
                FullName = "Чернова Екатерина Марковна",
                Age = 40,
                Gender = Gender.female,
                Mother = grandMother1,
                Father = grandFather1
            };

            FamilyMember father = new FamilyMember()
            {
                FullName = "Чернов Константин Тимофеевич",
                Age = 52,
                Gender = Gender.male,
                Mother = grandMother2,
                Father = grandFather2
            };

            FamilyMember son = new FamilyMember()
            {
                FullName = "Чернов Олег Константинович",
                Age = 20,
                Gender = Gender.male,
                Mother = mother,
                Father = father
            };

            FamilyMember daughter = new FamilyMember()
            {
                FullName = "Чернова Екатерина Константиновна",
                Age = 15,
                Gender = Gender.female,
                Mother = mother,
                Father = father
            };

            grandMother1.Daughter = mother;
            grandMother1.Husband = grandFather1;
            grandFather1.Daughter = mother;
            grandFather1.Wife = grandMother1;
            grandMother2.Son = father;
            grandMother2.Husband = grandFather2;
            grandFather2.Son = father;
            grandFather2.Wife = grandMother2;
            mother.Husband = father;
            mother.Son = son;
            mother.Daughter = daughter;
            father.Wife = mother;
            father.Daughter = daughter;
            father.Son = son;


            var GrandMothers = son.GetGrandMothers();
            var GrandFathers = daughter.GetGrandFathers();

            mother.ShowPartner();
            grandFather1.ShowPartner();

            foreach (var item in GrandFathers)
            {
                Console.WriteLine(item?.FullName ?? "Нет данных");
            }
            foreach (var item in GrandMothers)
            {
                Console.WriteLine(item?.FullName ?? "Нет данных");
            }
        }
    }
}
