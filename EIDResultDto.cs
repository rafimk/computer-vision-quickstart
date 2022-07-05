namespace computer_vision_quickstart
{
    public class EIDResultDto
    {
        public string IDNumber { get; set; }
        public string Name { get; set;}
        public DateOnly DateofBirth { get; set;}
        public DateOnly ExpiryDate { get; set; }
        public string CardNumber { get; set;}
    }
}