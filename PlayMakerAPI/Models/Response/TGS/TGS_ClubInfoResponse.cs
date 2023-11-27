namespace AlvivaAPI.Models.Response.TGS
{
    public class TGS_ClubInfoResponse
    {
        public string Result { get; set; }
        public TGS_ClubInfoData Data { get; set; }
    }

    public class TGS_ClubInfoData
    {
        public int OrgseasonID { get; set; }
        public int EventTypeID { get; set; }
        public bool IsOrgEvent { get; set; }
        public List<TGS_ClubInfoObj> ClubTeamList { get; set; }
    }

    public class TGS_ClubInfoObj
    {
        public string? ClubID { get; set; }
        public string? TeamID { get; set; }
        public string? RegisteredTeamName { get; set; }
        public string? TeamGender { get; set; }
        public string? TeamPhone { get; set; }
        public string? ClubName { get; set; }
        public int? EventID { get; set; }
        public string? DivisionName { get; set; }
        public int? DivisionID { get; set; }
        public int? FlightID { get; set; }
        public string? EcnlRecord { get; set; }
        public string? ClubLogo { get; set; }
        public string? Place { get; set; }
        public int? ExistingTeam { get; set; }
    }
}
