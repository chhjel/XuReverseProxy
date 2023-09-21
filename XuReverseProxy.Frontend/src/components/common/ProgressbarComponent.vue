<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from "vue-property-decorator";

@Options({
  components: {},
})
export default class ProgressbarComponent extends Vue {
  @Prop()
  value: number;

  @Prop()
  color: string;

  get innerStyle(): any {
    return {
      width: `${this.value}%`,
      "background-color": this.color,
    };
  }
}
</script>

<template>
  <div class="progress-bar">
    <div class="progress-bar--inner" :style="innerStyle"></div>
  </div>
</template>

<style scoped lang="scss">
.progress-bar {
  width: 100%;
  height: 3px;
  background-color: #92929263;

  &--inner {
    background-color: #fff;
    height: 100%;
    transition: 0.2s;
  }
}
</style>
