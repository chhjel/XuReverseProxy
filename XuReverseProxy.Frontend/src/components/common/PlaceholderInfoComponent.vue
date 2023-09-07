<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { PlaceholderGroupInfo, PlaceholderInfo } from "@utils/Constants";

@Options({
    components: {}
})
export default class PlaceholderInfoComponent extends Vue {
    @Prop()
    placeholders: Array<PlaceholderGroupInfo>;

    @Prop({ required: false, default: () => []})
    additionalPlaceholders: Array<PlaceholderInfo>

    allPlaceholderData: Array<PlaceholderInfo> = [];

    mounted(): void {
        this.additionalPlaceholders.forEach(d => {
            this.allPlaceholderData.push(d);
        })
        this.placeholders.forEach(p => {
            p.placeholders.forEach(d => {
                this.allPlaceholderData.push(d);
            })
        });
    }

    onInsertClicked(data: PlaceholderInfo) {
        this.$emit('insertPlaceholder', `{{${data.name}}}`);
    }
}
</script>

<template>
    <div class="placeholder-details">
        <table>
            <tr>
                <th>Name</th>
                <th>Description</th>
                <th>Insert</th>
            </tr>
            <tr v-for="data in allPlaceholderData">
                <td><code>&#123;&#123;{{ data.name }}&#125;&#125;</code></td>
                <td>{{ data.description }}</td>
                <td><a @click.prevent.stop="onInsertClicked(data)" href="#">[insert]</a></td>
            </tr>
        </table>
    </div>
</template>

<style scoped lang="scss">
.placeholder-details {
    table {
        text-align: left;
    }
    th {
        padding: 3px;
    }
    td {
        color: var(--color--text-dark);
        padding: 3px;
    }
    tr {
        border-bottom: 1px solid var(--color--text-darker);
        padding: 3px;
            
        &:nth-child(odd) {
            background-color: var(--color--table-odd);
        }
    }
}
</style>
