namespace GameClient.Modules.NameM
{
    public readonly struct Name
    {
        public string Surname { get; }
        public string LastName { get; }

        public Name(string surname, string lastName)
        {
            Surname = surname;
            LastName = lastName;
        }
        public string Text => Surname + LastName;
        public override string ToString() => Text;
    }
}