<template>
    <TeleportFix to="body">
    <div class="dialog-component" :class="rootClasses" :style="rootStyle" v-show="localValue">
        <div class="dialog-component_modal_wrapper" @click.self.prevent="onClickOutside" ref="modalWrapper">
            <div class="dialog-component_modal" :style="dialogStyle"
                @mousedown="onMouseDownInModal"
                @mouseup="onMouseUpInModal">
                <div class="dialog-component_modal_header" :class="headerColor">
                    <slot name="header"></slot>
                    <div class="spacer01"></div>
                    <slot name="headerRight"></slot>
                    <button-component @click="onClickClose" class="danger">
                        <div class="dialog-component_modal_header__closer material-icons icon">close</div>
                    </button-component>
                </div>
                <div class="dialog-component_modal_content" :style="contentStyle">
                    <slot></slot>
                </div>
                <div class="dialog-component_modal_footer" v-if="hasFooterSlot">
                    <slot name="footer"></slot>
                </div>
            </div>
        </div>
    </div>
    </TeleportFix>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { Teleport as teleport_, TeleportProps, VNodeProps } from 'vue';
import ValueUtils from '@utils/ValueUtils';
import EventBus, { CallbackUnregisterShortcut } from "@utils/EventBus";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";

const TeleportFix = teleport_ as {
  new (): {
    $props: VNodeProps & TeleportProps
  }
}
@Options({
    components: {
        TeleportFix,
        ButtonComponent
    },
    emits: ['close', 'update:value']
})
export default class DialogComponent extends Vue {
    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: false })
    hideOverlay!: string | boolean;

    @Prop({ required: false, default: false })
    persistent!: string | boolean;

    @Prop({ required: false, default: true })
    smartPersistent!: string | boolean;

    @Prop({ required: false, default: 800 })
    maxWidth!: number;

    @Prop({ required: false, default: false })
    fullWidth!: string | boolean;

    @Prop({ required: false, default: false })
    scrollableX!: string | boolean;

    @Prop({ required: false, default: null })
    width!: number | null;

    @Prop({ required: false, default: null })
    headerColor!: string | null;

    @Prop({ required: false, default: null })
    class!: string | null;

    @Ref() readonly modalWrapper!: HTMLElement;

    localValue: boolean = false;
    callbacks: Array<CallbackUnregisterShortcut> = [];
    static zIndexCounter: number = 1001;
    zIndex: number = 1001;
    static activeDialogCount: number = 0;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();

        this.callbacks = [
            EventBus.on("onEscapeClicked", this.onEscapeClicked.bind(this))
        ];
    }

    beforeUnmount(): void {
      this.callbacks.forEach(x => x.unregister());
    }

    ////////////////
    //  METHODS  //
    //////////////
    hasFooterSlot() { return !!this.$slots.footer; }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        let classes: any = {
             'hide-overlay': this.isHideOverlay,
             'persistent': this.isPersistent,
             'full-width': this.isFullWidth,
             'has-color': !!this.headerColor
        };
        if (this.class) classes[this.class] = true;
        return classes;
    }

    get rootStyle(): any {
        let style: any = {
            'z-index': this.zIndex
        };
        return style;
    }

    get dialogStyle(): any {
        let maxWidthValue = this.maxWidth?.toString() || '';
        if (this.maxWidth && maxWidthValue && !isNaN(Number(maxWidthValue))) {
            maxWidthValue = `${maxWidthValue}px`;
        }
        let widthValue = this.width?.toString() || '';
        if (this.width && widthValue && !isNaN(Number(widthValue))) {
            widthValue = `${widthValue}px`;
        }

        let style: any = {
            maxWidth: maxWidthValue ? `min(${maxWidthValue}, calc(100vw - 40px))` : null
        };
        if (widthValue) style['width'] = `min(${widthValue}, calc(100vw - 40px))`;
        return style;
    }

    get contentStyle(): any {
        // let maxWidthValue = this.maxWidth?.toString() || '';
        // if (this.maxWidth && maxWidthValue && !isNaN(Number(maxWidthValue))) {
        //     maxWidthValue = `${maxWidthValue}px`;
        // }
        // let widthValue = this.width?.toString() || '';
        // if (this.width && widthValue && !isNaN(Number(widthValue))) {
        //     widthValue = `${widthValue}px`;
        // }

        let style: any = {
            // maxWidth: maxWidthValue
        };
        if (this.isScrollableX) style['overflow-x'] = 'auto';

        return style;
    }

    get isHideOverlay(): boolean { return ValueUtils.IsToggleTrue(this.hideOverlay); }
    get isPersistent(): boolean { return ValueUtils.IsToggleTrue(this.persistent); }
    get isSmartPersistent(): boolean { return ValueUtils.IsToggleTrue(this.smartPersistent); }
    get isFullWidth(): boolean { return ValueUtils.IsToggleTrue(this.fullWidth); }
    get isScrollableX(): boolean { return ValueUtils.IsToggleTrue(this.scrollableX); }

    ////////////////
    //  METHODS  //
    //////////////
    public close(): void {
        const changed = this.localValue == true;
        this.localValue = false;
        if (changed) {
            this.onVisibilityChanged(this.localValue);
        }
        this.$emit("update:value", false);
        this.$emit("close", true);
    }

    shake(): void {
        this.clearShake();
        void this.modalWrapper.offsetWidth; // trigger reflow
        this.modalWrapper.classList.add('persistent-shake'); // start animation
    }

    clearShake(): void {
        this.modalWrapper.classList.remove('persistent-shake'); // reset animation
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onEscapeClicked(): void {
        if (this.isPersistent
            || (this.isSmartPersistent && document.activeElement?.classList?.contains('monaco-mouse-cursor-text') == true)) {
            this.shake();
            return;
        }
        this.close();
    }

    onClickClose(): void {
        this.close();
    }

    onVisibilityChanged(shown: boolean): void {
        DialogComponent.activeDialogCount = DialogComponent.activeDialogCount + (shown ? 1 : -1);
        document.body.style.overflow = DialogComponent.activeDialogCount == 0 ? null : 'hidden';
        this.clearShake();
    }

    onClickOutside(): void {
        if (this.mouseIsDownWithinModal) {
            this.mouseIsDownWithinModal = false;
            return;
        }
        else if (!this.isPersistent) this.close();
        else this.shake();
    }
    
    mouseIsDownWithinModal: boolean = false;
    onMouseDownInModal(): void {
        this.mouseIsDownWithinModal = true;
    }

    onMouseUpInModal(): void {
        this.mouseIsDownWithinModal = false;
    }
	
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
        const changed = this.localValue != this.value;

		this.localValue = this.value;
        if (this.localValue) {
            DialogComponent.zIndexCounter = DialogComponent.zIndexCounter + 1;
            this.zIndex = DialogComponent.zIndexCounter;
        }

        if (changed) {
            this.onVisibilityChanged(this.localValue);
        }
    }

    @Watch('localValue')
    emitLocalValue(): void
    {
		this.$emit('update:value', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.dialog-component {
    position: fixed;
    left: 0;
    top: 0;
    bottom: 0;
    right: 0;
    background-color: #0000005c;
    z-index: 1000;
    animation: dialog-open-bg .15s ease-in-out;

    &.hide-overlay {
        background-color: transparent;
    }
    /* &.persistent { }
    &.full-width { } */
    
    .dialog-component_modal_wrapper {
        height: 100%;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
        padding: 20px;
        animation: dialog-open .15s ease-in-out;

        &.persistent-shake {
            animation: dialog-persistent-shake 0.35s; // ease-in-out;
        }
        
        .dialog-component_modal {
            margin: 0 auto;
            /* margin: 0 !important; */
            margin-bottom: 40px;
            padding: 30px 0;
            transition: all 0.2s;
            box-shadow: 0 0 13px 5px #33333340;
            background: var(--color--dialog);
            border: 2px solid var(--color--primary);
            position: relative;
            max-width: 100%;
            overflow: hidden;
            display: flex;
            flex-direction: column;
            

            &_header {
                display: flex;
                align-items: center;
                justify-content: space-between;
                margin-top: -30px;
                padding: 10px 30px;
                font-size: 30px;
                font-weight: 600;
                min-height: 18px;
                box-sizing: border-box;
                background: var(--color--background-dark) !important;

                @media (max-width: 961px) {
                    padding: 5px;
                }

                &:empty {
                    display: none !important;
                }
                &__closer {
                    font-size: 30px;
                }
            }
            &_footer {
                margin-bottom: -30px;
                padding: 10px 30px;
                border-top: 2px solid var(--color--accent-darken9);
            }
            &_content {
                overflow-y: auto;
                overflow-x: hidden;
                max-height: calc(100vh - 190px);
                padding: 15px 30px;
                flex: 1;

                @media (max-width: 961px) {
                    padding: 5px;
                }
            }
        }
    }
}

@keyframes dialog-open-bg {
  0% {
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
}

@keyframes dialog-open {
  0% {
    opacity: 0;
    transform: translateY(50px);
  }
  100% {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes dialog-persistent-shake {
    0% { transform: translateX(0) }
    25% { transform: translateX(5px) }
    50% { transform: translateX(-5px) }
    75% { transform: translateX(5px) }
    100% { transform: translateX(0) }
}
</style>
