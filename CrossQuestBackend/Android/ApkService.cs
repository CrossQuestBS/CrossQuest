using System;
using System.Buffers.Text;
using System.IO;
using System.Resources;
using System.Threading.Tasks;
using CrossQuestBackend.Android.Models;

namespace CrossQuestBackend.Android;

public static class ApkService
{
    public static string Base64KeyFileContent = "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCG91Pzd8S07JnXvp19mewAXPhDLCvdsOTdK5/8pwBPt0Mr2+J2OqhQbD9S6j9bjWcjKGTpvvDKkltMxbBAARM5FprtQIXnJeiSA6o0m2po/iJGWSxSAlINNFCMOVydf7qto19oZJ3P6j6xjbL5bYGeTQ8CHP3jSe0m9W/D2A9ax1jBL8ZHQ5luNRFD9+Y+TQEyV9jY0At0GQLJW5E/PdE65cDVm3sWtWMqUknt9YY2k2FXPLm7KGnBqv3UsQJDnxCmH9flPs9FXcbu+sEp3zux/0lz2HOE6Dc86KpNJKPSwc406pXtbBxCQQikQWzCrUMpc+K48jKG74CBZD6hcunfAgMBAAECggEABMpzZ262mSQrkytH/zUJ2eVy8TzN0yZmqVcqwawn6FcoAj7hkLLPq13RovQqYD8/tvwbFj+vNRxG3g3HuNiQ7HOEolaqVM8qrJbcDCwBdff3aCndgeJlM+TJ1dW/bN541kBjWvBKDcmnL2RJQxiuWNvWuYt6k6kvIDU8IhE2/PuNet5dVNmVo6ZRRdPi0Z4k1eAyx4njWzPMGYHk66ujiCirHaNOQbHqpf9Ge8ij+GnTKQdJA62DcXW2ZxiLmqUrCagWWfelGE3bsR0lQ5oBXtp2/T2HIzrJGiyDQDjtpsUcgEEKqC92Oo2T6ituhSOF88dpvC3rHSKZnWS/iLBQ2QKBgQD107uXUioYnV9KVDDDLkdEHyZvmzjzwbWxRObFHoHI+vaB/uHoWZe8bXJBncDo4tUUeQVFr75sZ1neeBbq+dCGZ+kBVoCODcSR2UA3V5XIR8AKSShhXbQPg+YzTsH1o4YpcrQqm/BiHwaVkMq4ogLxDKYuUPE7D9QkiYWf/h1oiwKBgQCMjSU5DtKUq91qVizR+k+jeBmbKAV6ix80UnXHAPGHOgG9SGQd8eGjqoGPvBvYThSMdALrtCCdB7rRJCekH8t+YKT0e9Dh0HSQIaGgce6FQf0p/09fpq4obJI1nqnuAbzE7L10GMwfvmAkrcf8uxbssREIMeZp59Lnd/VCl8JafQKBgHHHLxMpr1w3MoyXjP45pDiOZl7PrDt+E9dZeaoQpaddKM0gKHU/SnCnA3QFTO09V7wjC2Kmpe9MopbKZGkbeP1MiNbar6OQEcQjlopG2oeZVfQsyijOkvF/bgOfVzyXFBiJA4SZKlhv3b9KBdoQ+mWRIjVbt1tLxzemAxf7KKdjAoGAGGdNZjnHoF6y5AqwX4j5mOV6dLEfOma7dUc4AeSNCzCsKqROFdDwn400T7OWlhkAgl6GP0yYOQuliTig1WNb3saC/Zwd6YdbJcdhG82MX4DUpx0YOABlzskDHeI9mQCeOQbt4iGIF57jbJrr1VraoSAhV+3qFstUmDIA2J4m9bUCgYEA1f/tmkdk7pQpvwoGzzVoyfWbDjvyndHgGuNoGH/0KaH3xSPHOA2j34pDAD3tCAuFl3keMulmUTNXjmA7hVqQACdcO0IRYQw/LY+n86j6TDmzOexR70OToDSWUBjzFdUyJh7IduhfEM84IfzLVOARbQ1zZyecIo66OLbBojCtXBE=";

    public static string Base64CertFileContent =
        "LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSUNwakNDQVk2Z0F3SUJBZ0lJY21PVmt1SS9EYlV3RFFZSktvWklodmNOQVFFTEJRQXdFakVRTUE0R0ExVUUKQXd3SFZXNXJibTkzYmpBZ0Z3MHhNVEE1TWprd01EQXdNREJhR0E4eU1EY3hNRGt5T1RBd01EQXdNRm93RWpFUQpNQTRHQTFVRUF3d0hWVzVyYm05M2JqQ0NBU0l3RFFZSktvWklodmNOQVFFQkJRQURnZ0VQQURDQ0FRb0NnZ0VCCkFJYjNVL04zeExUc21kZStuWDJaN0FCYytFTXNLOTJ3NU4wcm4veW5BRSszUXl2YjRuWTZxRkJzUDFMcVAxdU4KWnlNb1pPbSs4TXFTVzB6RnNFQUJFemtXbXUxQWhlY2w2SklEcWpTYmFtaitJa1paTEZJQ1VnMDBVSXc1WEoxLwp1cTJqWDJoa25jL3FQckdOc3ZsdGdaNU5Ed0ljL2VOSjdTYjFiOFBZRDFySFdNRXZ4a2REbVc0MUVVUDM1ajVOCkFUSlgyTmpRQzNRWkFzbGJrVDg5MFRybHdOV2JleGExWXlwU1NlMzFoamFUWVZjOHVic29hY0dxL2RTeEFrT2YKRUtZZjErVSt6MFZkeHU3NndTbmZPN0gvU1hQWWM0VG9OenpvcWswa285TEJ6alRxbGUxc0hFSkJDS1JCYk1LdApReWx6NHJqeU1vYnZnSUZrUHFGeTZkOENBd0VBQVRBTkJna3Foa2lHOXcwQkFRc0ZBQU9DQVFFQU1OVFFvOWxnCmJ2SG5wMU90NGcxVWdqcFNEdTUyQktkQUIwZWFlUi8zUnRtK0UwRStqVU1YU0k3MGltNFB4Yk4rZU9tVEczTkMKbzBuTy9GTFFVdzNqM28za21PTjRWbFBhcEdzRHBLZTJySGJMKzVIeVNQYlNqa0dwd1RUR1BWenpmaHY5ZFVENgpsOTdRSUI1Y212UkgzVDlDUC84Yytlck9BUkJGMmtHaXRkTlR0eVV4dlFzbC94YWlLQW51YUU3VWIwWW1wc1pRCmUxRWlKOUxOd0Y5Mll2SzNkV1A5Y0JLT0tueFFFQWNTZ3VnR1dXSWJpQ1dGOUtITFVXWXZUMkd2MXRnbCtrdkUKL1pVaWUrK09xbkZFalBlV0RUc2JwaUpYRDFzS0ZVcDNpQ2Y5NzBtZ0xNZlhZd2tpUnh3aWNZRm55MHR1OTB3RgpOYnp3eTF6S2hVQzgwdz09Ci0tLS0tRU5EIENFUlRJRklDQVRFLS0tLS0K";
    
    public static async Task<bool> ExtractApk(AndroidTools tools, string apkPath, string extractPath)
    {
        return await ProcessCaller.ProcessAsync("java", $"-jar \"{tools.ApktoolJar}\" d \"{apkPath}\" -o \"{extractPath}\" -f", true);
    }
    
    public static async Task<bool> CreateAPK(AndroidTools tools, string apkPath, string extractPath)
    {
        return await ProcessCaller.ProcessAsync("java", $"-jar \"{tools.ApktoolJar}\" b \"{extractPath}\" -o \"{apkPath}\" -f", true);
    }

    public static async Task<bool> SignAPK(AndroidTools tools, string apkPath)
    {
        var temporaryPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(temporaryPath);

        var certFile = "debug_cert.crt";
        var keyFile = "debug_key.pk8";

        var keyPath = Path.Join(temporaryPath, keyFile);
        var certPath = Path.Join(temporaryPath, certFile);


        await File.WriteAllBytesAsync(keyPath, Convert.FromBase64String(Base64KeyFileContent));
        await File.WriteAllBytesAsync(certPath, Convert.FromBase64String(Base64CertFileContent));

        var apkSignerPath = tools.Apksigner;
        var result = await ProcessCaller.ProcessAsync(
            apkSignerPath, 
            $"sign -v --key \"{keyPath}\" --cert \"{certPath}\" \"{apkPath}\"");

        Directory.Delete(temporaryPath, true);

        return result;
    }
    
}