using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccrualApp.Constants
{
    public class Mapping
    {
        public Dictionary<String , int> regionId()
        {

            Dictionary<String, int> regionMap = new Dictionary<String,int>();

            regionMap.Add("Advertising Consultants",1);
            regionMap.Add("California",2);
            regionMap.Add("CIPS Marketing Group",3);
            regionMap.Add("Interco",4);
            regionMap.Add("IPA Parcel Logistics",5);
            regionMap.Add("Last Mile Network",6);
            regionMap.Add("MDNET",7);
            regionMap.Add("Media Group",8);
            regionMap.Add("Midwest",9);
            regionMap.Add("Northeast",10);
            regionMap.Add("Northwest",11);
            regionMap.Add("Southeast",12);
            regionMap.Add("Southwest",13);
            regionMap.Add("Ultra Parcel Logistics",14);
            return regionMap;
        }
        public Dictionary<String, int> companyId() {

            Dictionary<String, int> companyMapping = new Dictionary<String, int>();

            companyMapping.Add("Advertising Consultants", 1);
            companyMapping.Add("California", 2);
            companyMapping.Add("Last Mile Network", 6);
            companyMapping.Add("MDNET", 7);
            companyMapping.Add("Midwest", 9);
            companyMapping.Add("Southeast", 12);
            companyMapping.Add("Southwest", 13);                  
            return companyMapping;
        }

        public Dictionary<String, int> customerId()
        {
            Dictionary<String, int> customerMapping = new Dictionary<String, int>();
            customerMapping.Add("PDQ", 3);
            customerMapping.Add("ACI Last Mile CA LLC", 113);
            customerMapping.Add("Acorn Newspapers", 171);
            customerMapping.Add("Agenti Media Services", 110);
            customerMapping.Add("Beach Reporter", 143);
            customerMapping.Add("Chino Champion", 416);
            customerMapping.Add("CIPS Marketing Group, LLC", 142);
            customerMapping.Add("Dow Jones & Company", 140);
            customerMapping.Add("Easy Reader", 109);
            customerMapping.Add("Gazette-Daily News", 150);
            customerMapping.Add("Greenleaf Guardian, LLC", 200);
            customerMapping.Add("Harbor Gateway N. Neighborhood Council", 151);
            customerMapping.Add("Hector Borboa - LAT SC", 136);
            customerMapping.Add("Hemet PE", 138);
            customerMapping.Add("HOY- FDS", 135);
            customerMapping.Add("LA Times", 106);
            customerMapping.Add("LA Times - CIPS", 130);
            customerMapping.Add("LA Times Santa Barbara-Barrons", 131);
            customerMapping.Add("LA Times Single Copy", 105);
            customerMapping.Add("Larchmont Chronicle", 154);
            customerMapping.Add("LAT/OC HD", 195);
            customerMapping.Add("Norwalk Patriot", 155);
            customerMapping.Add("OCR - NSD", 116);
            customerMapping.Add("OCR - Rack and Stack", 114);
            customerMapping.Add("Outlook/La Canada Flintridge", 156);
            customerMapping.Add("San Bernardino Sun", 157);
            customerMapping.Add("San Diego Neighborhood News - East County", 104);
            customerMapping.Add("San Diego News - Chula Vista Star News", 103);
            customerMapping.Add("San Diego Union-Tribune", 102);
            customerMapping.Add("San Pedro Today", 158);
            customerMapping.Add("Santa Barbara Daily Press", 201);
            customerMapping.Add("SCNG", 159);
            customerMapping.Add("SDUT HD-Other", 101);
            customerMapping.Add("South Bay Digs", 162);
            customerMapping.Add("Teak Santa Barbara", 163);
            customerMapping.Add("The Epoch Times Media Group LA", 415);
            customerMapping.Add("The Downey Patriot", 164);
            customerMapping.Add("UT Community Press", 115);
            customerMapping.Add("UT San Diego", 165); //recheck
            customerMapping.Add("Thryv, Inc.", 199);
            customerMapping.Add("Tracy Press", 125);
            customerMapping.Add("Valassis", 202);
            customerMapping.Add("Ventura County Star (HD)", 177);
            customerMapping.Add("Ventura County Star (TMC)", 124);
            customerMapping.Add("Victorville TMC", 167);
            customerMapping.Add("Venegas Distribution Inc.", 197);
            customerMapping.Add("First Mile", 262);
            customerMapping.Add("International Bridge", 268);
            customerMapping.Add("One Stop Mailing LLC", 265);
            customerMapping.Add("Last Mile Network, LLC", 263);
            customerMapping.Add("SFx", 408);
            customerMapping.Add("OpenE", 409);
            customerMapping.Add("One Live", 413);
            customerMapping.Add("YunExpress", 412);
            customerMapping.Add("Quad Logistics Holdings, LLC (Magazines)", 260);
            customerMapping.Add("Quad Logistics Holdings, LLC (Parcels)", 270);
            customerMapping.Add("Authorize.Net", 362);
            customerMapping.Add("Houston Chronicle", 339);
            customerMapping.Add("Cox Media Group Ohio", 378);
            customerMapping.Add("Shaw Media", 376);
            customerMapping.Add("Cox-Buyers Edge", 399);
            customerMapping.Add("Cox-Evening Edge", 396);
            customerMapping.Add("Palm Beach Post", 394);
            customerMapping.Add("Sun-Sentinel Company, LLC", 393);
            customerMapping.Add("Dallas Morning News, Inc.", 404);
            customerMapping.Add("Houston Chronicle Media Group", 401);
            customerMapping.Add("San Antonio Express-News", 402);
            customerMapping.Add("Golden State Newspapers LLC", 2);
            customerMapping.Add("SEKO-AirCity", 418);
            return customerMapping;
        }
          
        
    }
}
