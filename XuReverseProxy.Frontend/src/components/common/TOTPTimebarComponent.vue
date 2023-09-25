<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from "vue-property-decorator";

@Options({
  components: {},
})
export default class TOTPTimebarComponent extends Vue {
  value: number = 0;
  color: string = "orange";
  intervalRef: any = null;

  mounted(): void {
    this.updateValue();
    this.intervalRef = setInterval(() => this.updateValue(), 1000);
  }

  beforeUnmount(): void {
    clearInterval(this.intervalRef);
  }

  updateValue(): void {
    const secondsLeft = 30 - (Math.round(new Date().getTime() / 1000) % 30);
    this.value = Math.round((secondsLeft / 30) * 100);

    if (secondsLeft >= 10)  this.color = "var(--color--success-base)";
    else if (secondsLeft >= 5) this.color = "var(--color--warning-base)";
    else this.color = "var(--color--error-base)";
  }

  get innerStyle(): any {
    return {
      width: `${this.value}%`,
      "background-color": this.color,
    };
  }
}
</script>

<template>
  <div class="totp-bar">
    <div class="totp-bar--inner" :style="innerStyle"></div>
  </div>
</template>

<style scoped lang="scss">
.totp-bar {
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
