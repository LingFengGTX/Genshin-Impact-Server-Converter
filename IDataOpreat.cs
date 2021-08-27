namespace GenshinImpact_ServerConverter
{
    interface IDataOpreat
    {
        void LoadDefaultConfigure(string FilePath);//载入预设配置
        void ApplyTargetServer(int Target);//应用目标配置

        int GetServerType();//获取当前服务器类型 -1:未知,0:官方服,1:BiliBili
    }
}
