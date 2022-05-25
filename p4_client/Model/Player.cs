using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4_client.Model {
    public class Player {
        public string id { get; set; }
        public string name { get; set; }
        public Player(string player_infos) {
            this.name = player_infos.Split(":")[0];
            this.id = player_infos.Split(":")[1];
        }
    }
}
