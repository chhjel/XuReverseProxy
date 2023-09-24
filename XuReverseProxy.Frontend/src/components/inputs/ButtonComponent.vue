<script lang="ts">
import { Options } from "vue-class-component";
import { Prop, Vue, Watch } from "vue-property-decorator";
import ValueUtils from "@utils/ValueUtils";

@Options({
  components: {},
})
export default class ButtonComponent extends Vue {
  @Prop({ required: false, default: false })
  disabled: boolean;

  @Prop({ required: false, default: "" })
  href: string;

  @Prop({ required: false, default: "" })
  icon: string;

  @Prop({ required: false, default: false })
  secondary: boolean;

  @Prop({ required: false, default: false })
  danger: boolean;

  @Prop({ required: false, default: false })
  small: boolean;

  @Prop({ required: false, default: false })
  iconOnly: boolean;

  @Prop({ required: false, default: false })
  inProgress: boolean;

  localValue: string = "";

  mounted(): void {}

  get isDisabled(): boolean {
    return ValueUtils.IsToggleTrue(this.disabled);
  }
  get isSmall(): boolean {
    return ValueUtils.IsToggleTrue(this.small);
  }
  get isSecondary(): boolean {
    return ValueUtils.IsToggleTrue(this.secondary);
  }
  get isDanger(): boolean {
    return ValueUtils.IsToggleTrue(this.danger);
  }
  get isIconOnly(): boolean {
    return ValueUtils.IsToggleTrue(this.iconOnly);
  }
  get isInProgress(): boolean {
    return ValueUtils.IsToggleTrue(this.inProgress);
  }

  get rootClasses(): any {
    let classes: any = {
      disabled: this.isDisabled,
      secondary: this.isSecondary,
      danger: this.isDanger,
      small: this.isSmall,
      "icon-only": this.isIconOnly,
      "in-progress": this.isInProgress,
    };
    return classes;
  }

  onClick(): void {
    if (this.href) window.location.href = this.href;
  }
}
</script>

<template>
  <div class="button" :class="rootClasses" @click.stop.prevent="onClick" tabindex="0">
    <div class="material-icons icon" v-if="icon">{{ icon }}</div>
    <slot></slot>
  </div>
</template>

<style scoped lang="scss">
.button {
  font-size: 14px;
  font-weight: 500;
  display: inline-flex;
  flex-direction: row;
  align-content: center;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-transform: uppercase;
  text-decoration: none;
  cursor: pointer;
  border-radius: 2px;
  user-select: none;
  min-width: 88px;
  min-height: 36px;
  margin: 6px 8px;
  transition: 0.2s;
  background-color: var(--color--primary);
  padding: 0 5px;
  box-shadow:
    0 3px 1px -2px rgba(0, 0, 0, 0.2),
    0 2px 2px 0 rgba(0, 0, 0, 0.14),
    0 1px 5px 0 rgba(0, 0, 0, 0.12);
  text-align: center;
  color: var(--color--text);

  &:hover {
    background-color: var(--color--primary-lighten);
  }

  &.disabled {
    cursor: default;
    opacity: 0.5;
    pointer-events: none;
  }

  &.secondary {
    background-color: var(--color--secondary);

    &:hover {
      background-color: var(--color--secondary-lighten);
    }
  }
  &.danger {
    background-color: var(--color--danger);

    &:hover {
      background-color: var(--color--danger-lighten);
    }
  }

  &.small {
    font-size: 12px;
    font-weight: 400;
    min-width: 48px;
    min-height: 26px;

    .icon {
      font-size: 18px;
    }
  }

  &.icon-only {
    min-width: inherit;
    min-height: inherit;
    width: 32px;
    height: 32px;
    padding: 0;
  }

  .icon {
    transition: transform 0.5s;
  }
  &.in-progress {
    .icon {
      animation: spin-animation 0.75s infinite;
      color: var(--color--text-dark);
    }
  }

  @keyframes spin-animation {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(359deg);
    }
  }
}
</style>
