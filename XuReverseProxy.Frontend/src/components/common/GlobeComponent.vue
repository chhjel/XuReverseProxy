<template>
  <div class="globe-component" ref="rootElement">
    <div class="globe-component__globe" ref="globeElement"></div>
  </div>
</template>

<script lang="ts">
import { Vue, Prop, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import Globe, { GlobeInstance } from "globe.gl";

@Options({
  components: {},
})
export default class GlobeComponent extends Vue {
  @Prop({ required: false, default: 0 })
  lat!: number;

  @Prop({ required: false, default: 0 })
  lon!: number;

  @Prop({ required: false, default: 0.75 })
  altitude!: number;

  @Prop({ required: false, default: false })
  ping!: boolean;

  @Prop({ required: false, default: 2000 })
  focusDuration!: number;

  @Ref("globeElement")
  globeElement: HTMLElement;

  @Ref("rootElement")
  rootElement: HTMLElement;

  globe: GlobeInstance | null = null;

  mounted(): void {
    this.globe = Globe();
    this.globe(this.globeElement);
    this.configureGlobe();

    window.addEventListener("resize", this.onWindowResize);
    this.onWindowResize();
  }

  configureGlobe(): void {
    this.globe
      .globeImageUrl("//unpkg.com/three-globe@2.28.0/example/img/earth-blue-marble.jpg")
      .backgroundColor("#121212");

    const colorInterpolator = (t) => `rgba(255,100,50,${Math.sqrt(1 - t)})`;
    const ringPoints: Array<any> = [];
    if (this.ping && this.lat && this.lon) {
      ringPoints.push({
        lat: this.lat,
        lng: this.lon,
        maxR: 5,
        propagationSpeed: 1,
        repeatPeriod: 2000,
      });

      this.globe
        .ringsData(ringPoints)
        .ringColor(() => colorInterpolator)
        .ringResolution(8)
        .ringMaxRadius("maxR")
        .ringPropagationSpeed("propagationSpeed")
        .ringRepeatPeriod("repeatPeriod");

      this.globe.pointOfView({ lat: this.lat, lng: this.lon, altitude: this.altitude }, this.focusDuration);
    }
  }

  beforeDestroy(): void {
    window.removeEventListener("resize", this.onWindowResize);
  }

  onWindowResize(): void {
    this.globe.width(this.rootElement.clientWidth);
    this.globe.height(this.rootElement.clientHeight);
  }
}
</script>

<style scoped lang="scss">
/* .globe-component { } */
</style>
