Configuration ServiceClassConfigDisabled
{
   Import-DscResource -module MSFT_ServiceResourceClass
   # A Configuration block can have zero or more Node blocks
   Node "localhost"
   {
      # Next, specify one or more resource blocks
      ServiceResource myServiceSetting
      {                
          Name = "BITS"
          State = "Stopped"
          StartupType ="Disabled"
          BuiltInAccount="LocalSystem"
      }      
   }
} 