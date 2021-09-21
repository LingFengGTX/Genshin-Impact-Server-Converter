namespace GenshinImpact_ServerConverter
{
    interface IDataOpreat
    {
        /// <summary>
        /// 载入本地配置
        /// </summary>
        /// <param name="FilePath"></param>
        void LoadDefaultConfigure(string FilePath);//载入预设配置

        /// <summary>
        /// 执行指定服务器配置
        /// </summary>
        /// <param name="Target"></param>
        void ApplyTargetServer(int Target);//应用目标配置


        /// <summary>
        /// 获取当前游戏服务器类型(仅限配置数据)
        /// </summary>
        /// <returns>返回值由XML配置文件定义</returns>
        int GetServerType();

        /// <summary>
        /// 获取游戏相关的URL网址
        /// </summary>
        /// <param name="Index">索引项</param>
        /// <returns></returns>
        string GetServerURL(int Index);

        /// <summary>
        /// 获取游戏启动文件名
        /// </summary>
        /// <returns>文件名</returns>
        string StartupExe(int Index);

        /// <summary>
        /// 获取相关服务器的图标路径
        /// </summary>
        /// <param name="Index">索引项</param>
        /// <returns></returns>
        string GetServerIco(int Index);

        /// <summary>
        /// 根据目标索引返回目标代码
        /// </summary>
        /// <param name="Index">索引项</param>
        /// <returns></returns>
        string GetServerNameFromIndex(int Index);

        /// <summary>
        /// 获取定义的脚本文件名
        /// </summary>
        /// <returns>脚本文件名</returns>
        string GetScriptFileName();

        /// <summary>
        /// 指定项是否存在
        /// </summary>
        /// <param name="Title"></param>
        /// <returns>如果存在则为真，假则为不存在。</returns>
        int IfIndexIsTrue(string Title);
    }
}
