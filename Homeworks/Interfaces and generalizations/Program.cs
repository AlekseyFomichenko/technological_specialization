namespace Homeworks.Interfaces_and_generalizations
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //Bits bits = new(4);

            //Console.WriteLine(bits.GetBitByIndex(2));
            //bits.SetBitByIndex(0, true);
            //Console.WriteLine(bits.GetBitByIndex(0));

            //bits[1] = true;
            //Console.WriteLine(bits[1]);
            //Console.WriteLine(bits.Value);
            //Console.WriteLine(bits);
            //byte val = (byte)bits;
            //Console.WriteLine(val);

            Devices devices = new();
            Bits bits = new(1000);

            Console.WriteLine(devices);
            devices.TurnOnOff(bits);
            Console.WriteLine(devices);

            //Matrix<int> array = new Matrix<int>(2,2);

            //array[0,0] = 1;
            //array[0,1] = 2;
            //array[1,0] = 3;
            //array[1,1] = 4;

            //array.PrintMatrix();
        }
    }
}
