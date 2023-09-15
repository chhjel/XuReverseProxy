<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
// or import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
// if shipping only a subset of the features & languages is desired
import * as monaco from 'monaco-editor'
import DialogComponent from "@components/common/DialogComponent.vue";
import ButtonComponent from "./ButtonComponent.vue";
import IdUtils from "@utils/IdUtils";
import { isThisISOWeek } from "date-fns";

@Options({
    components: {
        DialogComponent,
        ButtonComponent
    }
})
export default class CodeInputComponent extends Vue {
    @Prop({ required: false, default: "json" })
    language!: string;

    @Prop({ required: false, default: '200px' })
    height!: string;

    @Prop({ required: false, default: "" })
    label!: string;

    @Prop({ required: false, default: false })
    readOnly!: boolean;

    @Prop({ required: false, default: false })
    wordWrap!: boolean;

    @Prop({ required: false, default: "vs-dark" }) // 'vs' (default), 'vs-dark', 'hc-black'
    theme!: string;

    @Prop({ required: false, default: "" })
    value!: string;

    @Prop({ required: false, default: "" })
    title!: string;

    @Ref('iframe')
    iframe: HTMLIFrameElement;

    isFullscreen: boolean = false;
    isEditorInited: boolean = false;
    isPreviewing: boolean = false;
    id: string = IdUtils.generateId();

    editor!: monaco.editor.IStandaloneCodeEditor;

    mounted(): void {
        this.configureMonacoEnv();
        this.editor = this.createMonacoEditor();

        window.addEventListener('resize', this.onWindowResize);
        setTimeout(() => {
            this.refreshSize();
        }, 10);
    }

    beforeDestroy(): void {
        window.removeEventListener('resize', this.onWindowResize)
    }

    public refreshSize(): void {
        if (this.editor) {
            this.editor.layout();
        }
    }

    public foldRegions(): void {
        this.editor.getAction("editor.foldAllMarkerRegions").run();
    }

    public insertText(val: string): void {
        this.editor.trigger('keyboard', 'type', {text: val});
    }

    getCursorPosition(): number {
        var model = this.editor.getModel();
        let pos = this.editor.getPosition();
        if (model == null || pos == null) return 0;
        
        return model.getOffsetAt({ lineNumber: pos.lineNumber, column: pos.column });
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        let classes: any = {
            'editor-readonly': this.readOnly,
            'fullscreen': this.isFullscreen
        };
        return classes;
    }

    get rootStyle(): any {
        return {
            height: this.height
        };
    }

    get allowPreviewHtml(): boolean { return this.language == 'html'; }
    
    ////////////////////////////////////////////////////////////
    //// EVENTHANDLERS /////////////////////////////////////////
    ////////////////////////////////////////////////////////////
    isNull: boolean = false;
    @Watch("value")
    onValueChanged(): void
    {
        this.isNull = (this.value == null);

        if (this.editor == null) return;
        
        const model = this.editor.getModel();
        if (model == null) return;
        else if (model.getValue() == this.value) return;

        model.setValue(this.value || '');
    }

    @Watch("readOnly")
    onReadOnlyChanged(): void
    {
        if (this.editor == null) return;
        this.editor.updateOptions({
            readOnly: this.readOnly
        });
    }

    onWindowResize(): void {
        this.refreshSize();
    }
    
    onEditorInit(): void {
        this.listenForChanges();

        this.$emit('editorInit', this.editor);
    }

    //////////////////////////////////////////////////////
    //// MONACO CONFIG //////////////////////////////////
    ////////////////////////////////////////////////////
    configureMonacoEnv(): void {
        (<monaco.Environment>(<any>self).MonacoEnvironment) =
        {
            getWorkerUrl: (moduleId: string, label: string): string =>
            {
                switch (label)
                {
                    case 'editorWorkerService': return "/dist/editor.worker.js";
                    case 'json': return "/dist/json.worker.js";
                    case 'html': return "/dist/html.worker.js";
                }
                return `/unknown/monaco/worker/${label}.js`;
            }
        }
    }

    createMonacoEditor(): monaco.editor.IStandaloneCodeEditor
    {
        const editor = monaco.editor.create(<HTMLElement>this.$refs.editorElement,
        {
            value: this.value,
            automaticLayout: true,
            language: this.language,
            readOnly: this.readOnly,
            glyphMargin: !this.readOnly,
            theme: this.theme,
            lineNumbersMinChars: 2,
            wordWrap: this.wordWrap ? "on" : "off"
        });
        
        const fitHeightToContent: boolean = false;
        if (fitHeightToContent)
        {
            editor.onDidChangeModelDecorations(() => {
                this.fitEditorHeightToContent()
                requestAnimationFrame(this.fitEditorHeightToContent)
            });
        }

        editor.onDidBlurEditorText(()=>{
            this.$emit('blur');
        });

        this.checkForInit();
        return editor;
    }

    checkForInit(): void {
        if (this.editor == undefined)
        {
            setTimeout(() => {
                this.checkForInit();
            }, 10);
            return;
        }

        this.isEditorInited = true;
        this.onEditorInit();
    }

    listenForChanges(): void {
        const model = this.editor.getModel();
        if (model == null) return;

        model.onDidChangeContent((e) => {
            let editorValue = model.getValue();
            if (this.isNull && !editorValue) editorValue = null;
            this.$emit('update:value', editorValue);
        })
    }

    // /**
    //  * Get an action that is a contribution to this editor.
    //  * @id Unique identifier of the contribution.
    //  * @return The action or null if action not found.
    //  */
    // getAction(id: string): IEditorAction;
    // /**
    //  * Execute a command on the editor.
    //  * The edits will land on the undo-redo stack, but no "undo stop" will be pushed.
    //  * @param source The source of the call.
    //  * @param command The command to execute
    //  */
    // executeCommand(source: string, command: ICommand): void;
    
    prevHeight: number = 0;
    fitEditorHeightToContent(): void
    {
        const editorElement = this.editor.getDomNode()

        if (!editorElement) {
            return
        }

        const lineHeight = this.editor.getOption(monaco.editor.EditorOption.lineHeight)
        const lineCount = this.editor.getModel()?.getLineCount() || 1
        const height = this.editor.getTopForLineNumber(lineCount + 1) + lineHeight

        if (this.prevHeight !== height) {
            this.prevHeight = height
            editorElement.style.height = `${height}px`
            this.editor.layout();
        }
    }

    togglePreview(): void {
        this.isPreviewing = !this.isPreviewing;
        if (this.isPreviewing) this.iframe.srcdoc = this.value;
    }
}
</script>

<template>
    <div>
        <div class="input-wrapper" v-if="label">
            <label>{{ label }}</label>
        </div>
        <div class="editor-component" :class="rootClasses" :style="rootStyle">
            <div class="editor-component__loader-bar" v-if="!isEditorInited">
                Loading...
            </div>
            <div class="editor-component__actions" v-if="!isFullscreen">
                <div v-if="allowPreviewHtml" @click="togglePreview">[preview]</div>
                <div @click="isFullscreen = true">[fullscreen]</div>
            </div>
            <div v-if="isFullscreen" class="editor-component__fullscreen-bar">
                <div class="spacer"></div>
                <div class="editor-component__actions-fullscreen">
                    <div v-if="allowPreviewHtml" @click="togglePreview">[preview]</div>
                    <div @click="isFullscreen = false">[Exit fullscreen]</div>
                </div>
            </div>

            <div ref="editorElement" class="editor-component__editor"></div>
        </div>
        
		<!-- Preview Dialog -->
		<dialog-component v-model:value="isPreviewing" max-width="2000">
			<template #header>HTML Preview</template>
			<template #footer>
				<button-component @click="isPreviewing = false" :disabled="readOnly" class="secondary">Close</button-component>
			</template>
            <div>
                <iframe :id="`${id}-preview`" ref="iframe"
                    style="width: calc(100vw - 50px); height: calc(100vh - 240px); border: 2px solid var(--color--secondary); background: #fff;"></iframe>
            </div>
		</dialog-component>
    </div>
</template>

<style scoped lang="scss">
.editor-component {
    position: relative;

    &__loader-bar {
        margin-bottom: -14px;
    }

    &__editor {
        width: 100%;
        height: 100%;
    }

    &.fullscreen {
        position: fixed;
        top: 32px;
        left: 0;
        right: 0;
        bottom: 0;
        z-index: 100;
        height: 100% !important;

        .editor-component__editor {
            height: calc(100% - 32px) !important;
        }
    }

    &__fullscreen-bar {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        z-index: 100;
        height: 32px;
        background-color: var(--color--panel-light);
        display: flex;
    }

    &__actions {
        cursor: pointer;
        position: absolute;
        top: -19px;
        right: -2px;
        background-color: var(--color--panel);
        z-index: 1;
        font-size: 12px;
        color: var(--color--secondary-lighten);
        padding: 2px;
        display: flex;

        > div {
            margin-left: 10px;

            &:hover {
                color: var(--color--info-base);
            }
        }
    }
    
    &__actions-fullscreen {
        display: flex;
        cursor: pointer;
        color: var(--color--secondary-lighten);
        font-size: 22px;
        
        > div {
            align-self: center;
            margin-right: 10px;
            
            &:hover {
                color: var(--color--info-base);
            }
        }
    }
}
</style>