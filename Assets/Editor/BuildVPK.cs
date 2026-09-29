using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BuildVPK {
    public static string DefaultBuildPath = "Builds/PSP2";
    public static string VitaIP = "192.168.1.100";
    public static int FTPPort = 1337;
    public static int CompanionPort = 1338;
    public static string FTPDestination = "ux0:/";

    [MenuItem("PSVita/Build VPK (Normal)")]
    [MenuItem("PSVita/Build VPK")]
    public static void BuildGameNormal() {
        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), DefaultBuildPath);
        string vpkName = Application.productName + ".vpk";
        string vpkPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Builds"), vpkName);

        BuildGame(buildPath, vpkPath);
        Debug.Log("PS Vita VPK Build completed successfully: " + vpkPath);
    }

    [MenuItem("PSVita/Build and Upload (FTP)")]
    public static void BuildGameFTP() {
        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), DefaultBuildPath);
        string vpkName = Application.productName + ".vpk";
        string vpkPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Builds"), vpkName);

        BuildGame(buildPath, vpkPath);
        UploadFTP(vpkPath, VitaIP, FTPPort, FTPDestination);
    }

    [MenuItem("PSVita/Build and Run (Companion)")]
    public static void BuildGameRun() {
        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), DefaultBuildPath);
        string vpkName = Application.productName + ".vpk";
        string vpkPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Builds"), vpkName);

        BuildGame(buildPath, vpkPath);
        UploadFTP(vpkPath, VitaIP, FTPPort, FTPDestination);
        string titleId = GetTitleID();
        RunCompanion(VitaIP, CompanionPort, titleId);
    }

    public static string BuildGame(string buildPath, string vpkPath) {
        if (Directory.Exists(buildPath)) {
            Directory.Delete(buildPath, true);
        }
        Directory.CreateDirectory(buildPath);

        // 1. Get enabled scenes
        string[] scenes = GetBuildScenes();
        if (scenes.Length == 0) {
            Debug.LogError("No scenes enabled in EditorBuildSettings!");
            return null;
        }

        // 2. Build Player targeting PSP2 (PC Hosted mode produces unencrypted ELF / TempBuild.self)
        Debug.Log("Building PSP2 Player to " + buildPath + "...");
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.PSP2, BuildOptions.None);

        // 3. Delete Junk
        DeleteJunk(buildPath);

        // 4. Remove Trial & Watermark, patch byte 0x80, rename to eboot.bin
        string selfFile = FindSelfFile(buildPath);
        if (!string.IsNullOrEmpty(selfFile) && File.Exists(selfFile)) {
            RemoveTrial(selfFile, true);
            string ebootPath = Path.Combine(buildPath, "eboot.bin");
            if (!selfFile.Equals(ebootPath, StringComparison.OrdinalIgnoreCase)) {
                if (File.Exists(ebootPath)) File.Delete(ebootPath);
                File.Move(selfFile, ebootPath);
            }
        } else {
            string ebootPath = Path.Combine(buildPath, "eboot.bin");
            if (File.Exists(ebootPath)) {
                RemoveTrial(ebootPath, true);
            }
        }

        // 5. Ensure sce_sys assets are present (param.sfo, icon0.png, livearea/contents/bg0.png, template.xml)
        EnsureSceSys(buildPath);

        // 6. Make VPK (Zip archive)
        string vpkDir = Path.GetDirectoryName(vpkPath);
        if (!Directory.Exists(vpkDir)) {
            Directory.CreateDirectory(vpkDir);
        }
        if (File.Exists(vpkPath)) {
            File.Delete(vpkPath);
        }
        MakeZip(buildPath, vpkPath);
        return vpkPath;
    }

    public static string[] GetBuildScenes() {
        EditorBuildSettingsScene[] sceneObjects = EditorBuildSettings.scenes;
        int count = 0;
        for (int i = 0; i < sceneObjects.Length; i++) {
            if (sceneObjects[i].enabled) count++;
        }
        string[] scenes = new string[count];
        int idx = 0;
        for (int i = 0; i < sceneObjects.Length; i++) {
            if (sceneObjects[i].enabled) {
                scenes[idx++] = sceneObjects[i].path;
            }
        }
        return scenes;
    }

    public static string FindSelfFile(string buildPath) {
        string tempBuildSelf = Path.Combine(buildPath, "TempBuild.self");
        if (File.Exists(tempBuildSelf)) return tempBuildSelf;

        string productSelf = Path.Combine(buildPath, Application.productName + ".self");
        if (File.Exists(productSelf)) return productSelf;

        string buildsSelf = Path.Combine(buildPath, "Builds.self");
        if (File.Exists(buildsSelf)) return buildsSelf;

        string[] selfFiles = Directory.GetFiles(buildPath, "*.self", SearchOption.TopDirectoryOnly);
        if (selfFiles.Length > 0) return selfFiles[0];

        return null;
    }

    public static void DeleteJunk(string buildPath) {
        string symbolPath = Path.Combine(buildPath, "SymbolFiles");
        if (Directory.Exists(symbolPath)) {
            Directory.Delete(symbolPath, true);
        }

        string configPath = Path.Combine(buildPath, "configuration.psp2path");
        if (File.Exists(configPath)) {
            File.Delete(configPath);
        }

        string batPath = Path.Combine(buildPath, "TempBuild.bat");
        if (File.Exists(batPath)) {
            File.Delete(batPath);
        }
    }

    public static void RemoveTrial(string selfFilePath, bool patchWatermark = true) {
        if (!File.Exists(selfFilePath)) return;

        using (FileStream fs = new FileStream(selfFilePath, FileMode.Open, FileAccess.ReadWrite)) {
            // Byte Offset 0x80: In Sony SELF executable header, offset 0x80 marks trial flag; write 0x00 to unlock
            if (fs.Length > 0x80) {
                fs.Seek(0x80, SeekOrigin.Begin);
                fs.WriteByte(0x00);
            }

            // Trial watermark patch: search for ASCII string "trial.png" and overwrite first character with 0x00
            if (patchWatermark) {
                byte[] pattern = Encoding.ASCII.GetBytes("trial.png");
                byte[] buffer = new byte[4096];
                long searchPos = 0;
                int bytesRead;

                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0) {
                    for (int i = 0; i <= bytesRead - pattern.Length; i++) {
                        bool match = true;
                        for (int j = 0; j < pattern.Length; j++) {
                            if (buffer[i + j] != pattern[j]) {
                                match = false;
                                break;
                            }
                        }
                        if (match) {
                            long foundOffset = searchPos + i;
                            fs.Seek(foundOffset, SeekOrigin.Begin);
                            fs.WriteByte(0x00); // Overwrite 't' with 0x00
                            fs.Seek(searchPos + bytesRead, SeekOrigin.Begin);
                        }
                    }
                    searchPos += bytesRead;
                }
            }
        }
    }

    public static void EnsureSceSys(string buildPath) {
        string destSceSys = Path.Combine(buildPath, "sce_sys");
        string destLiveArea = Path.Combine(Path.Combine(destSceSys, "livearea"), "contents");
        if (!Directory.Exists(destLiveArea)) {
            Directory.CreateDirectory(destLiveArea);
        }

        // Possible source folders for sce_sys
        string[] searchSources = new string[] {
            Path.Combine(Application.dataPath, "sce_sys"),
            Path.Combine(Path.Combine(Application.dataPath, "GodotAssets"), "sce_sys"),
            Path.Combine(Path.Combine(Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "NONNULL"), "Assets"), "GodotAssets"), "sce_sys"),
            Path.Combine(Path.Combine(Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "NONNULL"), "Temp"), "StagingArea"), "sce_sys")
        };

        string sourceSceSys = null;
        for (int i = 0; i < searchSources.Length; i++) {
            if (Directory.Exists(searchSources[i])) {
                sourceSceSys = searchSources[i];
                break;
            }
        }

        if (sourceSceSys != null) {
            CopyFileIfExists(Path.Combine(sourceSceSys, "param.sfo"), Path.Combine(destSceSys, "param.sfo"));
            CopyFileIfExists(Path.Combine(sourceSceSys, "icon0.png"), Path.Combine(destSceSys, "icon0.png"));
            CopyFileIfExists(Path.Combine(sourceSceSys, "pic0.png"), Path.Combine(destSceSys, "pic0.png"));
            
            string sourceLiveArea = Path.Combine(Path.Combine(sourceSceSys, "livearea"), "contents");
            if (Directory.Exists(sourceLiveArea)) {
                CopyFileIfExists(Path.Combine(sourceLiveArea, "bg0.png"), Path.Combine(destLiveArea, "bg0.png"));
                CopyFileIfExists(Path.Combine(sourceLiveArea, "template.xml"), Path.Combine(destLiveArea, "template.xml"));
                CopyFileIfExists(Path.Combine(sourceLiveArea, "startup.png"), Path.Combine(destLiveArea, "startup.png"));
                CopyFileIfExists(Path.Combine(sourceLiveArea, "default_gate.png"), Path.Combine(destLiveArea, "default_gate.png"));
            }
        }
    }

    private static void CopyFileIfExists(string source, string dest) {
        if (File.Exists(source)) {
            string dir = Path.GetDirectoryName(dest);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.Copy(source, dest, true);
        }
    }

    public static void MakeZip(string buildPath, string vpkPath) {
        // ZipFile not available in Unity 2018.2 .NET subset — use external zip
            var psi = new System.Diagnostics.ProcessStartInfo("zip", "-r \"" + vpkPath + "\" \"" + buildPath + "\"") { UseShellExecute = false, CreateNoWindow = true };
            System.Diagnostics.Process.Start(psi).WaitForExit();
    }

    public static string GetTitleID() {
        string contentID = PlayerSettings.PSVita.contentID;
        if (!string.IsNullOrEmpty(contentID) && contentID.Length >= 16) {
            // Format IV0000-ABCD12345_00-...
            int dashIndex = contentID.IndexOf('-');
            int underscoreIndex = contentID.IndexOf('_');
            if (dashIndex >= 0 && underscoreIndex > dashIndex) {
                return contentID.Substring(dashIndex + 1, underscoreIndex - dashIndex - 1);
            }
        }
        return "ABCD12345";
    }

    public static void UploadFTP(string vpkPath, string ip, int port, string destination) {
        if (!File.Exists(vpkPath)) {
            Debug.LogError("VPK file not found at " + vpkPath);
            return;
        }

        string fileName = Path.GetFileName(vpkPath);
        string ftpUrl = string.Format("ftp://{0}:{1}/{2}/{3}", ip, port, destination.Trim('/'), fileName);
        Debug.Log("Uploading " + fileName + " to " + ftpUrl + "...");

        try {
            using (WebClient client = new WebClient()) {
                client.UploadFile(ftpUrl, "STOR", vpkPath);
            }
            Debug.Log("FTP Upload successful!");
        } catch (Exception ex) {
            Debug.LogError("FTP Upload failed: " + ex.Message);
        }
    }

    public static void RunCompanion(string ip, int port, string titleId) {
        Debug.Log("Connecting to Vita Companion at " + ip + ":" + port + "...");
        try {
            using (TcpClient client = new TcpClient()) {
                client.Connect(ip, port);
                using (NetworkStream stream = client.GetStream()) {
                    SendCommand(stream, "screen on");
                    SendCommand(stream, "destroy");
                    SendCommand(stream, "launch " + titleId);
                }
            }
            Debug.Log("Companion commands sent successfully!");
        } catch (Exception ex) {
            Debug.LogError("Failed to connect to Vita Companion: " + ex.Message);
        }
    }

    private static void SendCommand(NetworkStream stream, string cmd) {
        byte[] bytes = Encoding.ASCII.GetBytes(cmd + "\n");
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush();
        System.Threading.Thread.Sleep(200);
    }
}
