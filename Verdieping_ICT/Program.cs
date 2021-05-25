using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Verdieping_ICT
{
    class Program
    {
        // Hier wordt een hash aangemaakt waar vervolgens mee wordt vergeleken in de code.
        // De code weet niet wat het daadwerkelijk wachtwoord is en moet dus zelf hashes maken en gaan vergelijken.
        // Er word GEEN gebruik gemaakt van SALT
        private static string _startHash;
        private static bool _isSalted;
        private static readonly Dictionary<string, string> UsedKeyCombos = new();
        private static int _estimatedHashes = 0;
        private static int _lastEstimatedHashes = 0;
        
        private static readonly string pathToHasFile =
            "C:\\Users\\Jouri\\RiderProjects\\Verdieping_ICT\\Verdieping_ICT\\hashes.txt";
        static async Task Main(string[] args)
        {
            Console.WriteLine("Type salthash of hash om te starten met brute forcing");
                var msg = Console.ReadLine();
                
                if (msg.Equals("salthash"))
                {
                    _startHash = GenerateSha256Hash.ComputeStringToSha256HashSalt("P3s2w0R8v4N70uR1");
                    _isSalted = true;
                }
                else
                {
                    _startHash = GenerateSha256Hash.ComputeStringToSha256Hash("P3s2w0R8v4N70uR1");
                    _isSalted = false;
                }
                Console.WriteLine($"Hash: {_startHash}");
                
                Thread processThread = new Thread(StartHashing);
                Thread metricsThread = new Thread(StartMetrics);
                if (msg is "salthash" or "hash")
                {
                    processThread.Start();
                    metricsThread.Start();
                }
        }
        
        // Hier wordt een metric bericht gestuurd elke 1 seconden om een update te geven hoeveel ram, ruimte en hashes per seconden
        static void StartMetrics()
        {
            while (true)
            {
                // Zet de thread in slaap voor 1 seconden
                Thread.Sleep(1000);
                
                // Verkrijg process informatie van draaiende software
                Process p = Process.GetCurrentProcess();
                
                // Verkrijg ram van process
                double ram = p.WorkingSet64;

                // Tel hoeveelheid bytes in de file
                long length = new FileInfo(pathToHasFile).Length;
                
                // Formateer bericht voor metric
                var metrics = $"RAM: {(ram/1024)/1024}MB \nEstimated Hashes Per Second: {_estimatedHashes - _lastEstimatedHashes} \nEstimated Total Hashes Generated: {_estimatedHashes} \nHash File Size: {length}Bytes";
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine(metrics);
                
                // Sla de geschatte hoeveelheid hashes op voor per seconden berekening
                _lastEstimatedHashes = _estimatedHashes;
            }
        }
        
        // Dit is een oneindige loop die constant nieuwe hashes genereerd en vergelijkt met de base hash
        // Elke hash wordt ook gestored in een dictionary (wegens O(1) access)
        // om te achterhalen of de hash al een keer eerder is genereerd (kans is zeeer klein)
        static async void StartHashing()
        {
            while (true)
            {
                // Generate een nieuwe hash
                var generatedHash = CompareAndFindCorrectHash.GenerateRandomHash(_isSalted);
                
                // Vergelijk hash met de start hash
                var isSame = CompareAndFindCorrectHash.IsHashTheSame(generatedHash.InputString, _startHash, _isSalted);
               
                // Check of de hash  gelijk is
                if (isSame)
                {
                    // Als het zelfde is wordt er een exception gegooid met de juiste hash en input
                    throw new Exception("Input: " + generatedHash.InputString + ", Output:" + generatedHash.OutputHash);
                }
                
                // Check of hash al in de dictionary zit
                if (UsedKeyCombos.ContainsKey(generatedHash.OutputHash))
                {
                    // Voeg hash toe aan dictionary als de hash nog niet in de dicationary zit
                    UsedKeyCombos.Add(generatedHash.OutputHash, generatedHash.InputString);
                }

                // Format input/ output hash for file storage
                var toString = $"{generatedHash.OutputHash} | {generatedHash.InputString}";
                
                // Tel hoeveelheid hashes
                _estimatedHashes++;
                
                // Async writer naar file voor visualisatie
                await using StreamWriter w = File.AppendText(pathToHasFile);
                await w.WriteLineAsync(toString);
                
                // Close connectie met file reader
                w.Close();
            }
        }
    }
}
