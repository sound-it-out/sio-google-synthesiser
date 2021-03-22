using Example.Testing.Attributes;

namespace Example.Tests
{
    public class WhenExample
    {
        [Then]
        public void UnitTest()
        {
            //This is a unit test which is exectued in the build and release github action
        }

        [Integration]
        public void IntegrationTest()
        {
            //This is a unit test which is only exectued in the release github action
        }
    }
}
