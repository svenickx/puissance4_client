using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace p4_client.Model {
    public class Player {
        public string Id { get; set; }
        public string Name { get; set; }
        public SolidColorBrush Color { get; set; }

        public Player(string player_infos, SolidColorBrush color) {
            this.Name = player_infos.Split(":")[0];
            this.Id = player_infos.Split(":")[1];
            this.Color = color;
        }
    }
}
