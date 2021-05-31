mkdir Dependencies

set DistancePath=C:\Program Files (x86)\Steam\steamapps\common\Distance\Distance_Data

mklink "Dependencies\Reactor.API.dll" "%DistancePath%\Centrifuge\Reactor.API.dll"
mklink "Dependencies\Centrifuge.Distance.dll" "%DistancePath%\Centrifuge\GameSupport\Centrifuge\Distance.dll"
mklink "Dependencies\UnityEngine.dll" "%DistancePath%\Managed\UnityEngine.dll"
mklink "Dependencies\Assembly-CSharp.dll" "%DistancePath%\Managed\Assembly-CSharp.dll"

pause