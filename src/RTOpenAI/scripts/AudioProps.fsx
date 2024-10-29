#r "nuget: NAudio.Core"

open System
open NAudio.Wave

let testFile = @"C:\Users\Faisa\AppData\Local\Packages\5D26BACE-A3ED-4E42-ABDA-1B0C073F7763_9zz4h110yvjzm\LocalCache\o551an5p.sog.wav"


let wave = new WaveFileReader(testFile)
wave.WaveFormat
wave.Close()
