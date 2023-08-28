<script lang="ts">
import { Options } from "vue-class-component";
import { Prop, Vue, Watch } from 'vue-property-decorator'
import ValueUtils from '@utils/ValueUtils';

@Options({
	components: { }
})
export default class ButtonComponent extends Vue {
    @Prop({ required: false, default: false })
    disabled: boolean;

    @Prop({ required: false, default: '' })
    href: string;

    @Prop({ required: false, default: false })
    secondary: boolean;

    @Prop({ required: false, default: false })
    danger: boolean;
    
    localValue: string = "";

    mounted(): void {
    }

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isSecondary(): boolean { return ValueUtils.IsToggleTrue(this.secondary); }
    get isDanger(): boolean { return ValueUtils.IsToggleTrue(this.danger); }

    get rootClasses(): any {
        let classes: any = {
            'disabled': this.isDisabled,
            'secondary': this.isSecondary,
            'danger': this.isDanger
        };
        return classes;
    }
    
    onClick(): void {
        if (this.href) window.location.href = this.href;
    }
}
</script>

<template>
  <div class="button" :class="rootClasses" @click.stop.prevent="onClick">
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

    &:not(.flat) {
        box-shadow: 0 3px 1px -2px rgba(0,0,0,.2),0 2px 2px 0 rgba(0,0,0,.14),0 1px 5px 0 rgba(0,0,0,.12);
    }

    a {
        color: var(--color--text);
        text-decoration: none;
        &:hover { text-decoration: none; }
        &:visited { color: var(--color--text); }
    }

    &__contents {
        display: flex;
        align-content: center;
        justify-content: flex-start;
        align-items: center;
        flex-direction: row;
        white-space: nowrap;
        padding: 5px 10px;
        text-overflow: ellipsis;
        overflow: hidden;
    }

    &:hover {
        background-color: var(--color--primary-lighten);
    }

    &.disabled {
        cursor: default;
        opacity: 0.8;
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

    // Styles
    &.outline {
        box-shadow: none !important;
    }
    &.round {
        border-radius: 50vh;
    }
    &.flat {
        background-color: transparent;
        box-shadow: none !important;
    }
}
</style>
