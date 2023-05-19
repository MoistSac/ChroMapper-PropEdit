using Beatmap.Base;
using SimpleJSON;

namespace ChroMapper_PropEdit.Components {

public interface iAnimateComponent {
	public iAnimateComponent PathAnimation(BaseObject bo, JSONArray points);
};

}
