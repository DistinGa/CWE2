using System.Collections.Generic;

namespace nsAI
{
    public interface ISuggestAISolution
    {
        List<AIGameAction> GetAISolutions(int authority);
        void RegisterInAIsystem();
        void UnRegisterInAIsystem();
    }
}
