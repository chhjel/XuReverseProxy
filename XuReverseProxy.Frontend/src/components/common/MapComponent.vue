<template>
  <div class="map-component">
    <div class="map-component__map" ref="mapElement"></div>
  </div>
</template>

<script lang="ts">
import { Vue, Prop, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import * as L from "leaflet";
import { nextTick } from "vue";

@Options({
  components: {},
})
export default class MapComponent extends Vue {
  @Prop({ required: false, default: 0 })
  lat!: number;

  @Prop({ required: false, default: 0 })
  lon!: number;

  @Prop({ required: false, default: 12 })
  zoom!: number;

  @Prop({ required: false, default: "" })
  note!: string;

  @Ref("mapElement")
  mapElement: HTMLElement;

  //////////////////
  //  LIFECYCLE  //
  ////////////////
  mounted(): void {
    const map = new L.Map(this.mapElement, {
      attributionControl: false,
    }).setView([this.lat, this.lon], this.zoom);

    L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
      maxZoom: 19,
      attribution: "",
    }).addTo(map);

    if (this.note) {
      const popup = L.popup().setLatLng([this.lat, this.lon]).setContent(this.note).openOn(map);
    }

    // Hacky "fix" for some gray issues
    nextTick(() => {
      window.dispatchEvent(new Event("resize"));
    });
  }

  ////////////////
  //  GETTERS  //
  //////////////
  ////////////////
  //  METHODS  //
  //////////////

  ///////////////////////
  //  EVENT HANDLERS  //
  /////////////////////

  /////////////////
  //  WATCHERS  //
  ///////////////
}
</script>

<style scoped lang="scss">
.map-component {
  &__map {
    height: 300px;
  }
}
</style>
