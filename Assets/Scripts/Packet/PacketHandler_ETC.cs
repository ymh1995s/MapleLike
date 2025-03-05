using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using Unity.VisualScripting;


// 기타 패킷 핸들러
public partial class PacketHandler
{
    public static void S_DropItemHandler(PacketSession session, IMessage packet)
    {
        //실제 로직 기능 구현 
        S_DropItem newitem = packet as S_DropItem;
        
    }
    
    /*
     *  몬스터를 죽임 -> 서버가  S_DropItem 패킷을  죽인사람한테 뿌림 ->  유니티가 S_DropItem를 받음 (button 기능) - > 유니티에서 S_DropItemHandler 자동으로 호출콜백  - > 수신 했으면  S_DropItemHandler여기서 기능 세부 구현  
     */
}
