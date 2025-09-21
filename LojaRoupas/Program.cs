using System;
using System.Globalization;

namespace ROUPAS
{
    internal class Program
    {
        static int contadorProd = 0;
        static int contadorVenda = 0;
        // [codigo, descricao, valor, quantidade]
        static string[,] produtos = new string[100, 4];
        // [codProduto, codVendedor, qtdVendida]
        static string[,] vendas = new string[100, 3];

        static readonly CultureInfo ptBR = new CultureInfo("pt-BR");

        static void Main(string[] args)
        {
            int opcao;
            do
            {
                Console.Clear();
                opcao = Menu();
                Console.Clear();

                switch (opcao)
                {
                    case 1: Cadastro(); break;
                    case 2: Venda(); break;
                    case 3: RelatorioVenda(); break;
                    case 4: RelatorioFuncionario(); break;
                    case 0: break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        Pausa();
                        break;
                }
            } while (opcao != 0);
        }

        static int Menu()
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("1 - Cadastrar produtos");
            Console.WriteLine("2 - Realizar uma venda");
            Console.WriteLine("3 - Relatório de vendas");
            Console.WriteLine("4 - Relatório de vendas por funcionários");
            Console.WriteLine("0 - Sair");
            Console.WriteLine("=========================================");
            Console.Write("Opção: ");

            if (!int.TryParse(Console.ReadLine(), out int opcao))
                opcao = -1;

            return opcao;
        }

        static void Cadastro()
        {
            if (contadorProd >= produtos.GetLength(0))
            {
                Console.WriteLine("Limite de produtos atingido.");
                Pausa();
                return;
            }

            Console.WriteLine("Cadastro de Produto:");
            Console.WriteLine("========================================");
            Console.Write("Informe o código do Produto: ");
            string codigo = (Console.ReadLine() ?? "").Trim();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                Console.WriteLine("Código inválido.");
                Pausa();
                return;
            }

            if (ExisteProduto(codigo) >= 0)
            {
                Console.WriteLine("Código já cadastrado! Tente novamente.");
                Pausa();
                return;
            }

            produtos[contadorProd, 0] = codigo;

            Console.Write("Informe a descrição do Produto: ");
            produtos[contadorProd, 1] = (Console.ReadLine() ?? "").Trim();

            Console.Write("Informe o valor do Produto (ex.: 99,90): ");
            if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Number, ptBR, out decimal valor) || valor < 0)
            {
                Console.WriteLine("Valor inválido.");
                Pausa();
                return;
            }
            produtos[contadorProd, 2] = valor.ToString(ptBR);

            Console.Write("Informe a quantidade do Produto: ");
            if (!int.TryParse(Console.ReadLine(), out int qtd) || qtd < 0)
            {
                Console.WriteLine("Quantidade inválida.");
                Pausa();
                return;
            }
            produtos[contadorProd, 3] = qtd.ToString();

            Console.WriteLine("========================================");
            Console.WriteLine("Produto cadastrado com sucesso!");
            Pausa();
            contadorProd++;
        }

        static void Venda()
        {
            if (contadorVenda >= vendas.GetLength(0))
            {
                Console.WriteLine("Limite de vendas atingido.");
                Pausa();
                return;
            }

            Console.WriteLine("Cadastro de Venda:");
            Console.WriteLine("========================================");
            Console.Write("Informe o código do Produto: ");
            string codigo = (Console.ReadLine() ?? "").Trim();

            int idxProd = ExisteProduto(codigo);
            if (idxProd >= 0)
            {
                Console.Write("Informe o código do Vendedor: ");
                string codigoVendedor = (Console.ReadLine() ?? "").Trim();

                Console.Write("Informe a quantidade vendida do Produto: ");
                if (!int.TryParse(Console.ReadLine(), out int quantidadeVendida) || quantidadeVendida <= 0)
                {
                    Console.WriteLine("Quantidade inválida.");
                    Pausa();
                    return;
                }

                bool atualizado = AtualizaEstoque(quantidadeVendida, idxProd);

                if (atualizado)
                {
                    vendas[contadorVenda, 0] = codigo;
                    vendas[contadorVenda, 1] = codigoVendedor;
                    vendas[contadorVenda, 2] = quantidadeVendida.ToString();

                    Console.WriteLine("========================================");
                    Console.WriteLine("Venda efetuada!");
                    contadorVenda++;
                }
                else
                {
                    Console.WriteLine("Estoque insuficiente. Venda cancelada.");
                }
            }
            else
            {
                Console.WriteLine("Produto não encontrado! Tente novamente.");
            }

            Pausa();
        }

        static void RelatorioVenda()
        {
            Console.WriteLine("Relatório de Vendas:");
            Console.WriteLine("========================================");

            if (contadorVenda == 0)
            {
                Console.WriteLine("Nenhuma venda registrada.");
                Pausa();
                return;
            }

            decimal totalGeral = 0m;

            for (int i = 0; i < contadorVenda; i++)
            {
                string cod = vendas[i, 0];
                int idx = ExisteProduto(cod);
                if (idx < 0) continue;

                string desc = produtos[idx, 1];
                decimal valor = decimal.Parse(produtos[idx, 2], ptBR);
                int qtd = int.Parse(vendas[i, 2]);

                decimal total = valor * qtd;
                totalGeral += total;

                Console.WriteLine($"Produto: {desc} (cod {cod}) | Qtd: {qtd} | Total: {total.ToString("C", ptBR)}");
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"TOTAL GERAL: {totalGeral.ToString("C", ptBR)}");
            Pausa();
        }

        static void RelatorioFuncionario()
        {
            Console.WriteLine("Relatório de Vendas por Funcionário:");
            Console.WriteLine("========================================");

            if (contadorVenda == 0)
            {
                Console.WriteLine("Nenhuma venda registrada.");
                Pausa();
                return;
            }

            string[] vendedores = new string[vendas.GetLength(0)];
            decimal[] totais = new decimal[vendas.GetLength(0)];
            int usados = 0;

            for (int i = 0; i < contadorVenda; i++)
            {
                string vend = vendas[i, 1];
                string cod = vendas[i, 0];
                int qtd = int.Parse(vendas[i, 2]);

                int idxP = ExisteProduto(cod);
                if (idxP < 0) continue;

                decimal valor = decimal.Parse(produtos[idxP, 2], ptBR);
                decimal total = valor * qtd;

                int pos = IndexOf(vendedores, usados, vend);
                if (pos < 0)
                {
                    vendedores[usados] = vend;
                    totais[usados] = total;
                    usados++;
                }
                else
                {
                    totais[pos] += total;
                }
            }

            const decimal COMISSAO = 0.05m;
            for (int i = 0; i < usados; i++)
            {
                decimal com = totais[i] * COMISSAO;
                Console.WriteLine($"Vendedor: {vendedores[i]} | Vendas: {totais[i].ToString("C", ptBR)} | Comissão (5%): {com.ToString("C", ptBR)}");
            }

            Pausa();
        }

        // ===== Helpers =====
        static int ExisteProduto(string codigo)
        {
            for (int i = 0; i < contadorProd; i++)
            {
                if (string.Equals(produtos[i, 0], codigo, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        static bool AtualizaEstoque(int quantidadeVendida, int idxProd)
        {
            int atual = int.Parse(produtos[idxProd, 3]);
            if (quantidadeVendida > atual) return false;

            int novo = atual - quantidadeVendida;
            produtos[idxProd, 3] = novo.ToString();
            return true;
        }

        static int IndexOf(string[] arr, int usados, string valor)
        {
            for (int i = 0; i < usados; i++)
                if (string.Equals(arr[i], valor, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        static void Pausa()
        {
            Console.WriteLine();
            Console.Write("Pressione qualquer tecla para continuar...");
            Console.ReadKey();
        }
    }
}
