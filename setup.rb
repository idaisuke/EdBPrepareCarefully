#! /usr/bin/env ruby

def dll_list
  if Gem::win_platform?
    # Windows
    require 'win32/registry'

    reg = Win32::Registry::HKEY_LOCAL_MACHINE.open('SOFTWARE\WOW6432Node\Valve\Steam')

    steam_path = File.join(reg['InstallPath'], 'steamapps')

    [
      File.join(steam_path, "common/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll"),
      File.join(steam_path, "common/RimWorld/RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll"),
      File.join(steam_path, "common/RimWorld/RimWorldWin64_Data/Managed/UnityEngine.IMGUIModule.dll"),
      File.join(steam_path, "common/RimWorld/RimWorldWin64_Data/Managed/UnityEngine.InputLegacyModule.dll"),
      File.join(steam_path, "common/RimWorld/RimWorldWin64_Data/Managed/UnityEngine.TextRenderingModule.dll"),
      File.join(steam_path, "workshop/content/294100/2009463077/Current/Assemblies/0Harmony.dll"),
    ]

  elsif /darwin/ =~ RUBY_PLATFORM
    # macOS
    steam_path = File.expand_path("~/Library/Application Support/Steam/steamapps")

    [
      File.join(steam_path, "common/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll"),
      File.join(steam_path, "common/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed/UnityEngine.CoreModule.dll"),
      File.join(steam_path, "common/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed/UnityEngine.IMGUIModule.dll"),
      File.join(steam_path, "common/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed/UnityEngine.InputLegacyModule.dll"),
      File.join(steam_path, "common/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed/UnityEngine.TextRenderingModule.dll"),
      File.join(steam_path, "workshop/content/294100/2009463077/Current/Assemblies/0Harmony.dll"),
    ]
  end
end

dll_list.each { |dll|
  target = dll
  link_name = File.expand_path("Dependencies/#{File.basename(dll)}", File.dirname(__FILE__))

  if File.exists?(link_name)
    File.delete(link_name)
  end

  puts "Create symlink Dependencies/#{File.basename(dll)}"
  File.symlink(target, link_name)
}
