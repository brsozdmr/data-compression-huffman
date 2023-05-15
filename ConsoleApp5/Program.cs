using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Resmi bmp olarak yükleme işlemi
        byte[] resimByte = File.ReadAllBytes(@"C:\Users\ozdem\Desktop\manzara.bmp");
        // Verileri sıkıştırma işlemi 
        byte[] sıkıstırılmısBytes = SıkıstırılmısVeri(resimByte);
        // Sıkıştırılmış verileri dosyaya kaydetme işlemi
        using (FileStream sıkıstırılmısDosya = File.Create("sıkıstırılmıs.dat"))
        {
            sıkıstırılmısDosya.Write(sıkıstırılmısBytes, 0, sıkıstırılmısBytes.Length);
        }
        // Sıkıştırılmış verileri okuma işlemi
        byte[] sıkıstırılmısVeriler= File.ReadAllBytes("sıkıstırılmıs.dat");
        // Sıkıştırılmış verileri çözümleme işlemi
        byte[] sıkıstırılmamısBytes = SıkıstırılmamısVeri(sıkıstırılmısVeriler);
        // Çözülmüş verileri dosyaya kaydetme işlemi
        using (FileStream sıkıstırılmamısDosya = File.Create("sıkıstırılmamıs.bmp"))
        {
            sıkıstırılmamısDosya.Write(sıkıstırılmamısBytes, 0, sıkıstırılmamısBytes.Length);
        }
        Console.WriteLine("Sıkıştırma işlemi tamamlandı.");
    }
    static byte[] SıkıstırılmısVeri(byte[] veri)
    {
        // Verileri Huffman aritmetik kodlama kullanarak sıkıştırma işlemi
        Dictionary<byte, int> frekans = new Dictionary<byte, int>();
        foreach (byte b in veri)
        {
            if (frekans.ContainsKey(b))
            {
                frekans[b]++;
            }
            else
            {
                frekans.Add(b, 1);
            }
        }
        List<Node> dugumler = new List<Node>();
        foreach (KeyValuePair<byte, int> kvp in frekans )
        {
            dugumler.Add(new Node(kvp.Key, kvp.Value));
        }
        Node kokdugum = HuffmanAgacı(dugumler);
        
        Dictionary<byte, string> encodingTable = new Dictionary<byte, string>();
        GenerateEncodingTable(kokdugum, "", encodingTable);

        List<bool> bitAktarımı = new List<bool>();
        foreach (byte b in veri)
        {
            string code = encodingTable[b];
            foreach (char c in code)
            {
                bitAktarımı.Add(c == '1');
            }
        }
        while (bitAktarımı.Count % 8 != 0)
        {
            bitAktarımı.Add(false);
        }

        byte[] sıkıstırılmısVeriler = new byte[bitAktarımı.Count / 8];
        for (int i = 0; i < bitAktarımı.Count; i += 8)
        {
            byte b = 0;
            for (int j = 0; j < 8; j++)
            {
                if (bitAktarımı[i + j])
                {
                    b |= (byte)(1 << (7 - j));
                }
            }
            sıkıstırılmısVeriler[i / 8] = b;
        }

        return sıkıstırılmısVeriler;
    }

    static byte[] SıkıstırılmamısVeri(byte[] sıkıstırılmısVeriler)
    {
        List<bool> bitAktarımı = new List<bool>();
        foreach (byte b in sıkıstırılmısVeriler)
        {
            for (int i = 0; i < 8; i++)
            {
                bitAktarımı.Add((b & (1 << (7 - i))) != 0);
            }
        }

        Node kokdugum = new Node();
        List<byte> sıkıstırılmamısVeriler = new List<byte>(); // Yeni bir byte listesi oluşturma işlemi

        int index = 0;
        while (index < bitAktarımı.Count)
        {
            Node node = kokdugum;
            while (!node.SonDugum())
            {
                if (bitAktarımı[index])
                {
                    node = node.SagChild;
                }
                else
                {
                    node = node.SolChild;
                }
                index++;
            }
            sıkıstırılmamısVeriler.Add(node.Deger);
        }

        return sıkıstırılmamısVeriler.ToArray();
    }


    static Node HuffmanAgacı(List<Node> dugumler)
    {
        while (dugumler.Count > 1)
        {
            dugumler.Sort((x, y) => x.Frekans.CompareTo(y.Frekans));

            Node solChild = dugumler[0];
            Node sagChild = dugumler[1];

            Node parentNode = new Node();
            parentNode.SolChild = solChild;
            parentNode.SagChild = sagChild;
            parentNode.Frekans = solChild.Frekans + sagChild.Frekans;

            dugumler.RemoveAt(0);
            dugumler.RemoveAt(0);

            dugumler.Add(parentNode);
        }
        return dugumler[0];
    }
    static void GenerateEncodingTable(Node node, string prefix, Dictionary<byte, string> encodingTable)
    {
        if (node.SonDugum())
        {
            encodingTable.Add(node.Deger, prefix);
            return;
        }
        GenerateEncodingTable(node.SolChild, prefix + "0", encodingTable);
        GenerateEncodingTable(node.SagChild, prefix + "1", encodingTable);
    }
}
//Node class ve işlemleri
class Node
{
    public byte Deger;
    public int Frekans;
    public Node SolChild;
    public Node SagChild;

    public Node()
    {
        Deger = 0;
        Frekans = 0;
        SolChild = null;
        SagChild = null;
    }
    public Node(byte value, int frequency)
    {
        Deger = value;
        Frekans = frequency;
        SolChild = null;
        SagChild = null;
    }
    public bool SonDugum()
    {
        return SolChild == null && SagChild == null;
    }
}

